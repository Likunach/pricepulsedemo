// PricePulse Landing Page JavaScript

document.addEventListener('DOMContentLoaded', function() {
    // Smooth scrolling for navigation links
    const navLinks = document.querySelectorAll('.nav-link[href^="#"]');
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Navbar scroll effect - disabled to keep purple color
    // const navbar = document.querySelector('.navbar');
    // window.addEventListener('scroll', function() {
    //     // Keep navbar purple always
    // });

    // Animate elements on scroll
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.animationDelay = '0.2s';
                entry.target.style.animationFillMode = 'both';
                entry.target.classList.add('animate-fade-in-up');
            }
        });
    }, observerOptions);

    // Observe elements for animation
    const animateElements = document.querySelectorAll('.feature-card, .benefit-item, .chart-container');
    animateElements.forEach(el => observer.observe(el));

    // Form validation and submission
    const demoForm = document.querySelector('.demo-form');
    if (demoForm) {
        demoForm.addEventListener('submit', function(e) {
            const email = this.querySelector('input[name="email"]').value;
            const company = this.querySelector('input[name="company"]').value;

            if (!email || !company) {
                e.preventDefault();
                alert('Please fill in all fields');
                return;
            }

            if (!isValidEmail(email)) {
                e.preventDefault();
                alert('Please enter a valid email address');
                return;
            }

            // Add loading state to button
            const submitBtn = this.querySelector('button[type="submit"]');
            submitBtn.textContent = 'Sending Request...';
            submitBtn.disabled = true;
        });
    }

    // Dashboard preview interaction
    const dashboardPreview = document.querySelector('.dashboard-preview');
    if (dashboardPreview) {
        dashboardPreview.addEventListener('mouseenter', function() {
            this.style.transform = 'rotateY(0deg) rotateX(0deg) scale(1.02)';
        });

        dashboardPreview.addEventListener('mouseleave', function() {
            this.style.transform = 'rotateY(-5deg) rotateX(5deg) scale(1)';
        });
    }

    // Counter animation for stats
    const statsCounters = document.querySelectorAll('.stat-number');
    const statsObserver = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateCounter(entry.target);
                statsObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.5 });

    statsCounters.forEach(counter => statsObserver.observe(counter));

    // Add click handlers for CTA buttons
    const ctaButtons = document.querySelectorAll('.btn-primary');
    ctaButtons.forEach(button => {
        if (button.textContent.includes('Demo')) {
            button.addEventListener('click', function() {
                const demoSection = document.getElementById('demo');
                if (demoSection) {
                    demoSection.scrollIntoView({ behavior: 'smooth' });
                }
            });
        }
    });
});

// Utility functions
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function animateCounter(element) {
    const target = element.textContent;
    const isPercentage = target.includes('%');
    const isTime = target.includes('/');
    const isNumber = target.includes(',') || target.includes('+');

    if (isNumber) {
        const finalValue = parseInt(target.replace(/[^\d]/g, ''));
        let currentValue = 0;
        const increment = finalValue / 50;
        const timer = setInterval(() => {
            currentValue += increment;
            if (currentValue >= finalValue) {
                currentValue = finalValue;
                clearInterval(timer);
            }
            element.textContent = Math.floor(currentValue).toLocaleString() + (target.includes('+') ? '+' : '');
        }, 40);
    } else if (isPercentage) {
        const finalValue = parseFloat(target);
        let currentValue = 0;
        const increment = finalValue / 50;
        const timer = setInterval(() => {
            currentValue += increment;
            if (currentValue >= finalValue) {
                currentValue = finalValue;
                clearInterval(timer);
            }
            element.textContent = currentValue.toFixed(1) + '%';
        }, 40);
    }
}

// Add CSS animation class
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeInUp {
        from {
            opacity: 0;
            transform: translateY(30px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
    
    .animate-fade-in-up {
        animation: fadeInUp 0.8s ease forwards;
    }
    
    .btn:active {
        transform: translateY(1px);
    }
    
    .feature-card:hover .feature-icon {
        transform: scale(1.1);
        transition: transform 0.3s ease;
    }
`;
document.head.appendChild(style);
