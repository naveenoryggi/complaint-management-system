// Production environment configuration
// The apiUrl can be overridden at runtime via assets/config.json
// - Windows Service mode: /api (API serves frontend at same origin)
// - IIS mode: /api/api (IIS virtual path + API routes)
export const environment = {
  production: true,
  apiUrl: '/api',  // Default for Windows Service; IIS mode uses config.json override
  apiTimeout: 30000
};

// Mutable config that can be updated at runtime
export const runtimeConfig = {
  apiUrl: '/api'
};

