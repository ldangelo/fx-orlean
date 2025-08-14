// Stripe payment processing JavaScript interop
let stripe = null;
let elements = null;
let paymentElement = null;

window.stripeInterop = {
    // Initialize Stripe with publishable key
    initialize: function (publishableKey) {
        try {
            stripe = Stripe(publishableKey);
            console.log('Stripe initialized successfully');
            return true;
        } catch (error) {
            console.error('Failed to initialize Stripe:', error);
            return false;
        }
    },

    // Create payment form elements
    createPaymentForm: function (elementId, clientSecret) {
        try {
            console.log('Creating payment form with element ID:', elementId);
            
            if (!stripe) {
                console.error('Stripe not initialized');
                return false;
            }

            const targetElement = document.getElementById(elementId);
            if (!targetElement) {
                console.error('DOM element not found:', elementId);
                return false;
            }

            console.log('Target element found:', targetElement);

            elements = stripe.elements({
                clientSecret: clientSecret,
                appearance: {
                    theme: 'stripe',
                    variables: {
                        colorPrimary: '#1976d2',
                        colorBackground: '#ffffff',
                        colorText: '#30313d',
                        colorDanger: '#df1b41',
                        fontFamily: 'Roboto, sans-serif',
                        spacingUnit: '2px',
                        borderRadius: '4px'
                    }
                }
            });

            paymentElement = elements.create('payment');
            paymentElement.mount(`#${elementId}`);
            
            console.log('Payment form created successfully');
            return true;
        } catch (error) {
            console.error('Failed to create payment form:', error);
            return false;
        }
    },

    // Confirm payment
    confirmPayment: async function (returnUrl) {
        try {
            if (!stripe || !elements) {
                throw new Error('Stripe or elements not initialized');
            }

            console.log('Confirming payment with return URL:', returnUrl);

            // Prepare confirmation parameters
            const confirmParams = {};
            
            // Only add return_url if we have a valid URL
            if (returnUrl && returnUrl !== 'about:blank' && returnUrl.startsWith('http')) {
                confirmParams.return_url = returnUrl;
            }

            const result = await stripe.confirmPayment({
                elements,
                confirmParams: confirmParams,
                redirect: 'if_required'
            });

            if (result.error) {
                console.error('Payment confirmation failed:', result.error);
                return {
                    success: false,
                    error: result.error.message
                };
            }

            console.log('Payment confirmed successfully:', result.paymentIntent);
            return {
                success: true,
                paymentIntentId: result.paymentIntent.id,
                status: result.paymentIntent.status
            };
        } catch (error) {
            console.error('Payment confirmation error:', error);
            return {
                success: false,
                error: error.message
            };
        }
    },

    // Create payment method for server-side processing
    createPaymentMethod: async function () {
        try {
            if (!stripe || !elements) {
                throw new Error('Stripe or elements not initialized');
            }

            const result = await stripe.createPaymentMethod({
                elements
            });

            if (result.error) {
                console.error('Failed to create payment method:', result.error);
                return {
                    success: false,
                    error: result.error.message
                };
            }

            console.log('Payment method created successfully:', result.paymentMethod);
            return {
                success: true,
                paymentMethodId: result.paymentMethod.id
            };
        } catch (error) {
            console.error('Create payment method error:', error);
            return {
                success: false,
                error: error.message
            };
        }
    },

    // Clean up elements
    destroy: function () {
        try {
            if (paymentElement) {
                paymentElement.destroy();
                paymentElement = null;
            }
            if (elements) {
                elements = null;
            }
            console.log('Stripe elements destroyed');
            return true;
        } catch (error) {
            console.error('Failed to destroy elements:', error);
            return false;
        }
    }
};