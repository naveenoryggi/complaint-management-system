// Check what master data endpoints return
const fs = require('fs');

async function checkEndpoints() {
    const token = fs.readFileSync('.test-token-temp', 'utf8').trim();
    const headers = { 'Authorization': `Bearer ${token}` };

    const endpoints = [
        '/api/master-data/categories',
        '/api/master-data/priorities',
        '/api/master-data/statuses',
        '/api/categories',
        '/api/priority-master',
        '/api/status-master'
    ];

    for (const endpoint of endpoints) {
        try {
            const response = await fetch(`http://localhost:5000${endpoint}`, { headers });
            const contentType = response.headers.get('content-type');
            const status = response.status;

            console.log(`\n${endpoint}:`);
            console.log(`  Status: ${status}`);
            console.log(`  Content-Type: ${contentType}`);

            if (status === 200) {
                const text = await response.text();
                if (text) {
                    const data = JSON.parse(text);
                    console.log(`  Data count: ${data.data?.length || 'N/A'}`);
                    if (data.data && data.data.length > 0) {
                        console.log(`  First item: ${JSON.stringify(data.data[0]).substring(0, 100)}...`);
                    }
                } else {
                    console.log(`  Response: EMPTY`);
                }
            }
        } catch (error) {
            console.log(`\n${endpoint}:`);
            console.log(`  Error: ${error.message}`);
        }
    }
}

checkEndpoints().catch(console.error);
