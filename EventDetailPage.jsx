import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import EventRegistrationButton from './EventRegistrationButton';

const EventDetailPage = () => {
    const { eventId } = useParams();
    const [eventData, setEventData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    
    // Get auth token from your auth system
    const authToken = localStorage.getItem('authToken') || 
                     sessionStorage.getItem('authToken');

    useEffect(() => {
        const fetchEventDetails = async () => {
            try {
                setLoading(true);
                const response = await fetch(`/api/events/${eventId}`);
                
                if (!response.ok) {
                    throw new Error('Failed to load event details');
                }
                
                const result = await response.json();
                setEventData(result.data);
            } catch (err) {
                setError(err.message);
                console.error('Failed to load event:', err);
            } finally {
                setLoading(false);
            }
        };

        if (eventId) {
            fetchEventDetails();
        }
    }, [eventId]);

    if (loading) {
        return (
            <div className="event-detail-page">
                <div className="loading-container">
                    <h2>Loading event details...</h2>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="event-detail-page">
                <div className="error-container">
                    <h2>Error Loading Event</h2>
                    <p>{error}</p>
                    <button onClick={() => window.location.reload()}>
                        Try Again
                    </button>
                </div>
            </div>
        );
    }

    if (!eventData) {
        return (
            <div className="event-detail-page">
                <div className="error-container">
                    <h2>Event Not Found</h2>
                    <p>The requested event could not be found.</p>
                </div>
            </div>
        );
    }

    return (
        <div className="event-detail-page">
            <div className="container">
                <header className="event-header">
                    <h1>{eventData.title}</h1>
                    <div className="event-meta">
                        <span className="event-type">{eventData.eventType}</span>
                        <span className="event-category">{eventData.categoryName}</span>
                    </div>
                </header>

                <div className="event-content">
                    <div className="event-main">
                        <section className="event-description">
                            <h2>About This Event</h2>
                            <p>{eventData.description}</p>
                        </section>

                        <section className="event-details">
                            <h2>Event Details</h2>
                            <div className="details-grid">
                                <div className="detail-item">
                                    <strong>Date & Time:</strong>
                                    <span>
                                        {new Date(eventData.startDateTime).toLocaleString()} - 
                                        {new Date(eventData.endDateTime).toLocaleString()}
                                    </span>
                                </div>
                                <div className="detail-item">
                                    <strong>Venue:</strong>
                                    <span>{eventData.venue}</span>
                                </div>
                                <div className="detail-item">
                                    <strong>Address:</strong>
                                    <span>
                                        {eventData.address}
                                        {eventData.city && `, ${eventData.city}`}
                                        {eventData.country && `, ${eventData.country}`}
                                    </span>
                                </div>
                            </div>
                        </section>
                    </div>

                    <aside className="event-sidebar">
                        <div className="event-stats">
                            <h3>Event Information</h3>
                            <div className="stat-item">
                                <span className="stat-label">Capacity:</span>
                                <span className="stat-value">{eventData.capacity}</span>
                            </div>
                            <div className="stat-item">
                                <span className="stat-label">Registered:</span>
                                <span className="stat-value">{eventData.currentRegistrations}</span>
                            </div>
                            <div className="stat-item">
                                <span className="stat-label">Available:</span>
                                <span className="stat-value">{eventData.remainingCapacity}</span>
                            </div>
                            <div className="stat-item">
                                <span className="stat-label">Registration:</span>
                                <span className={`stat-value ${eventData.isRegistrationOpen ? 'open' : 'closed'}`}>
                                    {eventData.isRegistrationOpen ? 'Open' : 'Closed'}
                                </span>
                            </div>
                        </div>

                        {/* This is where the magic happens! */}
                        <EventRegistrationButton
                            eventId={parseInt(eventId)}
                            authToken={authToken}
                            apiBaseUrl="/api"
                        />
                        
                        {eventData.primaryImageUrl && (
                            <div className="event-image">
                                <img 
                                    src={eventData.primaryImageUrl} 
                                    alt={eventData.title}
                                    onError={(e) => {
                                        e.target.style.display = 'none';
                                    }}
                                />
                            </div>
                        )}
                    </aside>
                </div>
            </div>
        </div>
    );
};

export default EventDetailPage; 