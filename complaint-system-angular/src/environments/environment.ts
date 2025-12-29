export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  apiTimeout: 30000,
  timezone: 'Asia/Kolkata',
  dateFormat: 'dd/MM/yyyy',
  timeFormat: 'hh:mm a'
};

// Mutable config that can be updated at runtime (for consistency with production)
export const runtimeConfig = {
  apiUrl: 'http://localhost:5000/api'
};
