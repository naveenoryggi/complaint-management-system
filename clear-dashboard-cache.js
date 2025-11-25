// Run this in browser console (F12) to clear dashboard cache
console.log('Clearing dashboard cache...');

// Get current user ID
const userIdFromToken = sessionStorage.getItem('currentUser');
const userId = userIdFromToken ? JSON.parse(userIdFromToken).id : null;

console.log('User ID:', userId);

// Clear all dashboard-related localStorage items
const keysToRemove = [];
for (let i = 0; i < localStorage.length; i++) {
    const key = localStorage.key(i);
    if (key && (key.includes('dashboard') || key.includes('preferences') || key.includes('statistics') || key.includes('widget'))) {
        keysToRemove.push(key);
    }
}

console.log('Removing keys:', keysToRemove);
keysToRemove.forEach(key => localStorage.removeItem(key));

console.log('Dashboard cache cleared! Now refresh the page (F5).');
