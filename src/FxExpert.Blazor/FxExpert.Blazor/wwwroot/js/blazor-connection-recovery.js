// Blazor Server SignalR Connection Recovery
// This script helps maintain Blazor Server connectivity and recover from connection issues

(function () {
    console.log('🔌 Blazor connection recovery script loaded');
    
    let reconnectAttempts = 0;
    let maxReconnectAttempts = 5;
    let reconnectDelay = 2000; // Start with 2 seconds
    let maxReconnectDelay = 30000; // Maximum 30 seconds
    let isManualReconnect = false;
    
    // Wait for Blazor to be available
    function waitForBlazor() {
        if (typeof Blazor !== 'undefined' && Blazor.start) {
            setupConnectionRecovery();
        } else {
            setTimeout(waitForBlazor, 100);
        }
    }
    
    function setupConnectionRecovery() {
        console.log('🚀 Setting up Blazor connection recovery');
        
        // Override the default Blazor reconnection UI
        Blazor.defaultReconnectionHandler = {
            onConnectionDown: () => {
                console.log('🔴 Blazor connection lost');
                showConnectionStatus('disconnected');
                return true; // Suppress default UI
            },
            onConnectionUp: () => {
                console.log('🟢 Blazor connection restored');
                showConnectionStatus('connected');
                reconnectAttempts = 0;
                reconnectDelay = 2000;
            }
        };
        
        // Monitor for circuit failures and attempt recovery
        window.addEventListener('beforeunload', function() {
            console.log('🚪 Page unloading, cleaning up connection');
        });
        
        // Detect when interactive elements stop working
        let lastInteractionTime = Date.now();
        
        // Update interaction time on any user input
        document.addEventListener('click', () => {
            lastInteractionTime = Date.now();
        });
        
        document.addEventListener('input', () => {
            lastInteractionTime = Date.now();
        });
        
        // Check connection health every 10 seconds
        setInterval(checkConnectionHealth, 10000);
    }
    
    function checkConnectionHealth() {
        // If it's been more than 5 minutes since last interaction, don't check
        if (Date.now() - lastInteractionTime > 300000) {
            return;
        }
        
        // Simple check by trying to invoke a Blazor method
        if (typeof DotNet !== 'undefined' && DotNet.invokeMethodAsync) {
            try {
                // This will fail if the circuit is broken
                DotNet.invokeMethodAsync('FxExpert.Blazor', 'CheckConnection')
                    .catch(() => {
                        console.log('⚠️ Circuit health check failed - connection may be broken');
                        attemptReconnection();
                    });
            } catch (error) {
                console.log('⚠️ Circuit health check failed with exception:', error);
                attemptReconnection();
            }
        }
    }
    
    function attemptReconnection() {
        if (isManualReconnect || reconnectAttempts >= maxReconnectAttempts) {
            return;
        }
        
        isManualReconnect = true;
        reconnectAttempts++;
        
        console.log(`🔄 Attempting reconnection #${reconnectAttempts}`);
        showConnectionStatus('reconnecting', reconnectAttempts);
        
        setTimeout(() => {
            if (typeof Blazor !== 'undefined' && Blazor.reconnect) {
                Blazor.reconnect()
                    .then(() => {
                        console.log('✅ Successful manual reconnection');
                        isManualReconnect = false;
                        showConnectionStatus('connected');
                        reconnectAttempts = 0;
                        reconnectDelay = 2000;
                    })
                    .catch(() => {
                        console.log(`❌ Manual reconnection attempt ${reconnectAttempts} failed`);
                        isManualReconnect = false;
                        
                        if (reconnectAttempts < maxReconnectAttempts) {
                            // Exponential backoff with jitter
                            reconnectDelay = Math.min(
                                reconnectDelay * (1.5 + Math.random() * 0.5),
                                maxReconnectDelay
                            );
                            setTimeout(attemptReconnection, reconnectDelay);
                        } else {
                            console.log('❌ All reconnection attempts failed');
                            showConnectionStatus('failed');
                        }
                    });
            } else {
                console.log('❌ Blazor.reconnect not available, trying page reload');
                if (reconnectAttempts >= maxReconnectAttempts) {
                    showConnectionStatus('reload-required');
                } else {
                    setTimeout(() => location.reload(), 1000);
                }
            }
        }, Math.min(reconnectDelay, maxReconnectDelay));
    }
    
    function showConnectionStatus(status, attempt = 0) {
        // Remove any existing status elements
        const existingStatus = document.getElementById('blazor-connection-status');
        if (existingStatus) {
            existingStatus.remove();
        }
        
        let message = '';
        let className = 'blazor-connection-status';
        
        switch (status) {
            case 'disconnected':
                message = '🔴 Connection lost - attempting to reconnect...';
                className += ' disconnected';
                break;
            case 'reconnecting':
                message = `🔄 Reconnecting (attempt ${attempt}/${maxReconnectAttempts})...`;
                className += ' reconnecting';
                break;
            case 'connected':
                message = '🟢 Connection restored';
                className += ' connected';
                setTimeout(() => {
                    const element = document.getElementById('blazor-connection-status');
                    if (element) element.remove();
                }, 3000);
                break;
            case 'failed':
                message = '❌ Connection failed - please refresh the page';
                className += ' failed';
                break;
            case 'reload-required':
                message = '🔄 Connection lost - please refresh the page to continue';
                className += ' reload-required';
                break;
        }
        
        if (message) {
            const statusElement = document.createElement('div');
            statusElement.id = 'blazor-connection-status';
            statusElement.className = className;
            statusElement.innerHTML = `
                <div style="position: fixed; top: 0; left: 0; right: 0; z-index: 9999; 
                           background: #f44336; color: white; padding: 8px; text-align: center; 
                           font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', sans-serif;">
                    ${message}
                    ${status === 'failed' || status === 'reload-required' ? 
                        '<button onclick="location.reload()" style="margin-left: 10px; background: white; color: #f44336; border: none; padding: 4px 8px; border-radius: 4px; cursor: pointer;">Refresh Page</button>' : 
                        ''}
                </div>
            `;
            document.body.appendChild(statusElement);
        }
    }
    
    // Start monitoring when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', waitForBlazor);
    } else {
        waitForBlazor();
    }
    
    // Expose manual reconnect function globally
    window.blazorReconnect = function() {
        console.log('🔧 Manual reconnection requested');
        reconnectAttempts = 0;
        attemptReconnection();
    };
    
})();