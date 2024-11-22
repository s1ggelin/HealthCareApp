document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById('loginform');
    const userIdInput = document.getElementById('userId');

    console.log('Login Form:', loginForm);
    console.log('User ID Input:', userIdInput);

    if (loginForm) {
        loginForm.addEventListener('submit', function () {
            if (userIdInput) {
                const userId = userIdInput.value;
                console.log('Attempting to save userId:', userId);

                if (userId) {
                    localStorage.setItem('userId', userId);
                    console.log("User ID saved to localStorage:", localStorage.getItem('userId'));
                } else {
                    console.warn('No userId found');
                }
            }
        });
    }
});