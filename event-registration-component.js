// Event Registration Component
class EventRegistrationManager {
    constructor(eventId, apiBaseUrl = '/api') {
        this.eventId = eventId;
        this.apiBaseUrl = apiBaseUrl;
        this.userRegistration = null;
        this.eventData = null;
        this.isLoading = false;
    }

    // Initialize the component
    async init() {
        this.isLoading = true;
        this.updateButtonState();
        
        try {
            await Promise.all([
                this.loadEventData(),
                this.loadUserRegistrationStatus()
            ]);
        } catch (error) {
            console.error('Failed to initialize registration manager:', error);
        }
        
        this.isLoading = false;
        this.updateButtonState();
    }

    // Load event details
    async loadEventData() {
        try {
            const response = await fetch(`${this.apiBaseUrl}/events/${this.eventId}`);
            if (!response.ok) throw new Error('Failed to load event data');
            
            const result = await response.json();
            this.eventData = result.data;
        } catch (error) {
            console.error('Error loading event data:', error);
            throw error;
        }
    }

    // Check if user is registered for this event
    async loadUserRegistrationStatus() {
        try {
            const response = await fetch(`${this.apiBaseUrl}/registrations/my-registrations`, {
                headers: {
                    'Authorization': `Bearer ${this.getAuthToken()}`
                }
            });
            
            if (!response.ok) {
                if (response.status === 401) {
                    // User not authenticated
                    return;
                }
                throw new Error('Failed to load registration status');
            }
            
            const result = await response.json();
            
            // Find registration for current event that is active
            this.userRegistration = result.data.items.find(reg => 
                reg.eventId === this.eventId && 
                reg.isActive && 
                !reg.isCancelled
            );
            
        } catch (error) {
            console.error('Error loading registration status:', error);
            // Don't throw - user might not be authenticated
        }
    }

    // Register for the event
    async registerForEvent(notes = '') {
        if (this.isLoading || this.userRegistration) return;
        
        this.isLoading = true;
        this.updateButtonState();
        
        try {
            const response = await fetch(`${this.apiBaseUrl}/registrations`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.getAuthToken()}`
                },
                body: JSON.stringify({
                    eventId: this.eventId,
                    notes: notes
                })
            });
            
            if (!response.ok) {
                const errorResult = await response.json();
                throw new Error(errorResult.message || 'Registration failed');
            }
            
            const result = await response.json();
            
            // Refresh registration status
            await this.loadUserRegistrationStatus();
            await this.loadEventData(); // Refresh event data for updated capacity
            
            this.showSuccessMessage('Successfully registered for the event!');
            
        } catch (error) {
            console.error('Registration failed:', error);
            this.showErrorMessage(error.message);
        }
        
        this.isLoading = false;
        this.updateButtonState();
    }

    // Cancel registration
    async cancelRegistration() {
        if (this.isLoading || !this.userRegistration) return;
        
        if (!confirm('Are you sure you want to cancel your registration?')) {
            return;
        }
        
        this.isLoading = true;
        this.updateButtonState();
        
        try {
            const response = await fetch(`${this.apiBaseUrl}/registrations/${this.userRegistration.id}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.getAuthToken()}`
                },
                body: JSON.stringify({
                    reason: 'User requested cancellation'
                })
            });
            
            if (!response.ok) {
                const errorResult = await response.json();
                throw new Error(errorResult.message || 'Cancellation failed');
            }
            
            // Clear local registration data
            this.userRegistration = null;
            
            // Refresh event data for updated capacity
            await this.loadEventData();
            
            this.showSuccessMessage('Registration cancelled successfully!');
            
        } catch (error) {
            console.error('Cancellation failed:', error);
            this.showErrorMessage(error.message);
        }
        
        this.isLoading = false;
        this.updateButtonState();
    }

    // Update button appearance and text
    updateButtonState() {
        const button = document.getElementById('registration-button');
        const statusText = document.getElementById('registration-status');
        
        if (!button) return;
        
        // Remove all state classes
        button.classList.remove('registered', 'loading', 'full', 'closed');
        
        if (this.isLoading) {
            button.disabled = true;
            button.textContent = 'Loading...';
            button.classList.add('loading');
            return;
        }
        
        // Check if user is authenticated
        if (!this.getAuthToken()) {
            button.textContent = 'Login to Register';
            button.disabled = true;
            if (statusText) statusText.textContent = 'Please log in to register for events';
            return;
        }
        
        // Check event availability
        if (this.eventData) {
            if (!this.eventData.isRegistrationOpen) {
                button.textContent = 'Registration Closed';
                button.disabled = true;
                button.classList.add('closed');
                if (statusText) statusText.textContent = 'Registration is no longer available';
                return;
            }
            
            if (this.eventData.remainingCapacity <= 0) {
                button.textContent = 'Event Full';
                button.disabled = true;
                button.classList.add('full');
                if (statusText) statusText.textContent = 'This event has reached capacity';
                return;
            }
        }
        
        // Check registration status
        if (this.userRegistration) {
            button.textContent = 'Registered ✓';
            button.disabled = false;
            button.classList.add('registered');
            if (statusText) {
                statusText.textContent = `Registered on ${new Date(this.userRegistration.registeredAt).toLocaleDateString()}`;
            }
        } else {
            button.textContent = 'Register for Event';
            button.disabled = false;
            if (statusText && this.eventData) {
                statusText.textContent = `${this.eventData.remainingCapacity} spots remaining`;
            }
        }
    }

    // Set up event listeners
    setupEventListeners() {
        const button = document.getElementById('registration-button');
        if (button) {
            button.addEventListener('click', () => {
                if (this.userRegistration) {
                    this.cancelRegistration();
                } else {
                    // You can add a modal here to collect notes
                    this.registerForEvent();
                }
            });
        }
    }

    // Helper method to get auth token (implement based on your auth system)
    getAuthToken() {
        return localStorage.getItem('authToken') || 
               sessionStorage.getItem('authToken') ||
               null;
    }

    // Show success message
    showSuccessMessage(message) {
        // Implement your preferred notification method
        alert(message); // Replace with your notification system
    }

    // Show error message
    showErrorMessage(message) {
        // Implement your preferred notification method
        alert('Error: ' + message); // Replace with your notification system
    }
}

// Usage example
document.addEventListener('DOMContentLoaded', async function() {
    // Get event ID from URL or data attribute
    const eventId = getEventIdFromPage(); // Implement this based on your routing
    
    if (eventId) {
        const registrationManager = new EventRegistrationManager(eventId);
        registrationManager.setupEventListeners();
        await registrationManager.init();
    }
});

// Helper function to extract event ID from page context
function getEventIdFromPage() {
    // Method 1: From URL parameter
    const urlParams = new URLSearchParams(window.location.search);
    const eventIdFromUrl = urlParams.get('eventId') || urlParams.get('id');
    
    // Method 2: From data attribute
    const eventElement = document.querySelector('[data-event-id]');
    const eventIdFromAttribute = eventElement?.getAttribute('data-event-id');
    
    // Method 3: From URL path (e.g., /events/123)
    const pathMatch = window.location.pathname.match(/\/events\/(\d+)/);
    const eventIdFromPath = pathMatch ? pathMatch[1] : null;
    
    return parseInt(eventIdFromUrl || eventIdFromAttribute || eventIdFromPath);
} 