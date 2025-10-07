// This is a simplified validation script for demonstration purposes.
const form = document.getElementById('validationForm');
const email = document.getElementById('email');
const password = document.getElementById('password');
const confirmPassword = document.getElementById('confirm-password');
const emailError = document.getElementById('email-error');
const passwordError = document.getElementById('password-error');
const confirmPasswordError = document.getElementById('confirm-password-error');

function validateField(input, errorElement, validationFn, errorMessage) {
    const isValid = validationFn(input.value);
    const parent = input.parentElement;
    const validIcon = parent.querySelector('.is-valid');
    const errorIcon = parent.querySelector('.has-error');

    if (input.value.length === 0) {
        input.classList.remove('has-error', 'is-valid');
        errorElement.classList.add('hidden');
        validIcon.classList.add('hidden');
        errorIcon.classList.add('hidden');
        return null;
    }

    if (isValid) {
        input.classList.remove('has-error');
        input.classList.add('is-valid');
        errorElement.classList.add('hidden');
        validIcon.classList.remove('hidden');
        errorIcon.classList.add('hidden');
    } else {
        input.classList.remove('is-valid');
        input.classList.add('has-error');
        errorElement.textContent = errorMessage;
        errorElement.classList.remove('hidden');
        validIcon.classList.add('hidden');
        errorIcon.classList.remove('hidden');
    }

    return isValid;
}

const validateEmail = function(value) {
    return value.indexOf('@') > 0 && value.indexOf('.') > value.indexOf('@') && value.indexOf(' ') === -1;
};
const validatePassword = function(value) { return value.length >= 8; };
const validateConfirmPassword = function(value) { return value === password.value && value.length > 0; };

email.addEventListener('input', function() { validateField(email, emailError, validateEmail, 'Please enter a valid email address.'); });
password.addEventListener('input', function() {
    validateField(password, passwordError, validatePassword, 'Password must be at least 8 characters long.');
    validateField(confirmPassword, confirmPasswordError, validateConfirmPassword, 'Passwords do not match.');
});
confirmPassword.addEventListener('input', function() { validateField(confirmPassword, confirmPasswordError, validateConfirmPassword, 'Passwords do not match.'); });

form.addEventListener('submit', function (event) {
    event.preventDefault();
    const isEmailValid = validateField(email, emailError, validateEmail, 'Please enter a valid email address.');
    const isPasswordValid = validateField(password, passwordError, validatePassword, 'Password must be at least 8 characters long.');
    const isConfirmPasswordValid = validateField(confirmPassword, confirmPasswordError, validateConfirmPassword, 'Passwords do not match.');

    if (isEmailValid && isPasswordValid && isConfirmPasswordValid) {
        alert('Form submitted successfully!');
        form.reset();
        document.querySelectorAll('.form-input').forEach(function(input) {
            input.classList.remove('is-valid', 'has-error');
            const parent = input.parentElement;
            parent.querySelector('.is-valid').classList.add('hidden');
            parent.querySelector('.has-error').classList.add('hidden');
        });
        document.querySelectorAll('p[id$="-error"]').forEach(function(errorEl) {
            errorEl.classList.add('hidden');
        });
    }
});
