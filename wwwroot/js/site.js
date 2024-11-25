

document.addEventListener("DOMContentLoaded", () => {
    const userIdInput = document.getElementById("userId");
    if (userIdInput) {
        const userId = userIdInput.value;
        localStorage.setItem("userId", userId);
        console.log("User ID saved to localStorage:", userId);
    }
});
