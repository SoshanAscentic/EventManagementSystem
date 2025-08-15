import React, { useState, useEffect, useCallback } from 'react';
import './EventRegistrationButton.css';

const EventRegistrationButton = ({ eventId, apiBaseUrl = '/api', authToken }) => {
    const [isLoading, setIsLoading] = useState(false);
    const [eventData, setEventData] = useState(null);
    const [userRegistration, setUserRegistration] = useState(null);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);

    // Fetch event details
    const fetchEventData = useCallback(async () => {
        try {
            const response = await fetch(`${apiBaseUrl}/events/${eventId}`);
            if (!response.ok) throw new Error('Failed to load event data');
            
            const result = await response.json();
            setEventData(result.data);
        } catch (err) {
            console.error('Error loading event data:', err);
            setError('Failed to load event details');
        }
    }, [apiBaseUrl, eventId]);

    // Fetch user registration status
    const fetchUserRegistrationStatus = useCallback(async () => {
        if (!authToken) return;

        try {
            const response = await fetch(`${apiBaseUrl}/registrations/my-registrations`, {
                headers: {
                    'Authorization': `Bearer ${authToken}`
                }
            });
            
            if (!response.ok) {
                if (response.status === 401) return; // User not authenticated
                throw new Error('Failed to load registration status');
            }
            
            const result = await response.json();
            
            // Find registration for current event that is active
            const registration = result.data.items.find(reg => 
                reg.eventId === eventId && 
                reg.isActive && 
                !reg.isCancelled
            );
            
            setUserRegistration(registration || null);
            
        } catch (err) {
            console.error('Error loading registration status:', err);
            // Don't set error state - user might not be authenticated
        }
    }, [apiBaseUrl, eventId, authToken]);

    // Initialize component
    useEffect(() => {
        const init = async () => {
            setIsLoading(true);
            setError(null);
            
            try {
                await Promise.all([
                    fetchEventData(),
                    fetchUserRegistrationStatus()
                ]);
            } catch (err) {
                console.error('Failed to initialize:', err);
            }
            
            setIsLoading(false);
        };

        if (eventId) {
            init();
        }
    }, [eventId, fetchEventData, fetchUserRegistrationStatus]);

    // Register for event
    const handleRegister = async (notes = '') => {
        if (isLoading || userRegistration || !authToken) return;
        
        setIsLoading(true);
        setError(null);
        setSuccess(null);
        
        try {
            const response = await fetch(`${apiBaseUrl}/registrations`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${authToken}`
                },
                body: JSON.stringify({
                    eventId: eventId,
                    notes: notes
                })
            });
            
            if (!response.ok) {
                const errorResult = await response.json();
                throw new Error(errorResult.message || 'Registration failed');
            }
            
            // Refresh data
            await Promise.all([
                fetchEventData(),
                fetchUserRegistrationStatus()
            ]);
            
            setSuccess('Successfully registered for the event!');
            
        } catch (err) {
            console.error('Registration failed:', err);
            setError(err.message);
        }
        
        setIsLoading(false);
    };

    // Cancel registration
    const handleCancel = async () => {
        if (isLoading || !userRegistration || !authToken) return;
        
        if (!window.confirm('Are you sure you want to cancel your registration?')) {
            return;
        }
        
        setIsLoading(true);
        setError(null);
        setSuccess(null);
        
        try {
            const response = await fetch(`${apiBaseUrl}/registrations/${userRegistration.id}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${authToken}`
                },
                body: JSON.stringify({
                    reason: 'User requested cancellation'
                })
            });
            
            if (!response.ok) {
                const errorResult = await response.json();
                throw new Error(errorResult.message || 'Cancellation failed');
            }
            
            // Clear local registration data and refresh event data
            setUserRegistration(null);
            await fetchEventData();
            
            setSuccess('Registration cancelled successfully!');
            
        } catch (err) {
            console.error('Cancellation failed:', err);
            setError(err.message);
        }
        
        setIsLoading(false);
    };

    // Determine button state and content
    const getButtonConfig = () => {
        if (isLoading) {
            return {
                text: 'Loading...',
                disabled: true,
                className: 'loading',
                onClick: null
            };
        }
        
        if (!authToken) {
            return {
                text: 'Login to Register',
                disabled: true,
                className: 'auth-required',
                onClick: null
            };
        }
        
        if (eventData) {
            if (!eventData.isRegistrationOpen) {
                return {
                    text: 'Registration Closed',
                    disabled: true,
                    className: 'closed',
                    onClick: null
                };
            }
            
            if (eventData.remainingCapacity <= 0) {
                return {
                    text: 'Event Full',
                    disabled: true,
                    className: 'full',
                    onClick: null
                };
            }
        }
        
        if (userRegistration) {
            return {
                text: 'Registered ✓',
                disabled: false,
                className: 'registered',
                onClick: handleCancel
            };
        }
        
        return {
            text: 'Register for Event',
            disabled: false,
            className: 'available',
            onClick: handleRegister
        };
    };

    // Get status message
    const getStatusMessage = () => {
        if (error) return error;
        if (success) return success;
        
        if (!authToken) return 'Please log in to register for events';
        
        if (eventData) {
            if (!eventData.isRegistrationOpen) {
                return 'Registration is no longer available';
            }
            
            if (eventData.remainingCapacity <= 0) {
                return 'This event has reached capacity';
            }
        }
        
        if (userRegistration) {
            return `Registered on ${new Date(userRegistration.registeredAt).toLocaleDateString()}`;
        }
        
        if (eventData) {
            return `${eventData.remainingCapacity} spots remaining`;
        }
        
        return '';
    };

    const buttonConfig = getButtonConfig();
    const statusMessage = getStatusMessage();

    return (
        <div className="event-registration-container">
            <button
                className={`event-registration-button ${buttonConfig.className}`}
                disabled={buttonConfig.disabled}
                onClick={buttonConfig.onClick}
            >
                {buttonConfig.text}
            </button>
            
            {statusMessage && (
                <div className={`registration-status ${error ? 'error' : success ? 'success' : 'info'}`}>
                    {statusMessage}
                </div>
            )}
        </div>
    );
};

export default EventRegistrationButton; 