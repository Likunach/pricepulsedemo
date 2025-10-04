document.addEventListener('DOMContentLoaded', function() {
    const passwordInput = document.getElementById('password');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const setupBtn = document.getElementById('setupBtn');
    
    const requirements = {
        length: document.getElementById('length-req'),
        uppercase: document.getElementById('uppercase-req'),
        lowercase: document.getElementById('lowercase-req'),
        number: document.getElementById('number-req'),
        special: document.getElementById('special-req'),
        match: document.getElementById('match-req')
    };

    function validatePassword() {
        const password = passwordInput.value;
        const confirmPassword = confirmPasswordInput.value;
        
        // Length check
        const hasLength = password.length >= 8;
        updateRequirement(requirements.length, hasLength);
        
        // Uppercase check
        const hasUppercase = /[A-Z]/.test(password);
        updateRequirement(requirements.uppercase, hasUppercase);
        
        // Lowercase check
        const hasLowercase = /[a-z]/.test(password);
        updateRequirement(requirements.lowercase, hasLowercase);
        
        // Number check
        const hasNumber = /\d/.test(password);
        updateRequirement(requirements.number, hasNumber);
        
        // Special character check
        const hasSpecial = password.includes('!') || password.includes('@') || password.includes('#') || 
                          password.includes('$') || password.includes('%') || password.includes('^') || 
                          password.includes('&') || password.includes('*') || password.includes('(') || 
                          password.includes(')') || password.includes('_') || password.includes('+') || 
                          password.includes('-') || password.includes('=') || password.includes('[') || 
                          password.includes(']') || password.includes('{') || password.includes('}') || 
                          password.includes('|') || password.includes(';') || password.includes(':') || 
                          password.includes(',') || password.includes('.') || password.includes('<') || 
                          password.includes('>') || password.includes('?');
        updateRequirement(requirements.special, hasSpecial);
        
        // Match check
        const passwordsMatch = password === confirmPassword && password.length > 0;
        updateRequirement(requirements.match, passwordsMatch);
        
        // Enable button if all requirements are met
        const allValid = hasLength && hasUppercase && hasLowercase && hasNumber && hasSpecial && passwordsMatch;
        setupBtn.disabled = !allValid;
    }

    function updateRequirement(element, isValid) {
        if (isValid) {
            element.classList.add('valid');
            element.classList.remove('invalid');
        } else {
            element.classList.add('invalid');
            element.classList.remove('valid');
        }
    }

    passwordInput.addEventListener('input', validatePassword);
    confirmPasswordInput.addEventListener('input', validatePassword);
});
