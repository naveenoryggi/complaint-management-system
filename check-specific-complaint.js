// Check specific complaint by ID
const fs = require('fs');

async function checkComplaint() {
    // Login to get fresh token
    const loginResponse = await fetch('http://localhost:5000/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            email: 'admin@complaintmanagement.com',
            password: 'Admin@123'
        })
    });
    const loginData = await loginResponse.json();
    const token = loginData.data.token;

    console.log('Checking complaint by ID...\n');

    // Check the specific complaint we just created
    const complaintId = 'b13be5c1-6e55-4a70-ac2d-9f5e935501e3';
    const response1 = await fetch(`http://localhost:5000/api/complaints/${complaintId}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    const data1 = await response1.json();
    console.log('Get by ID result:', data1.isSuccess ? 'SUCCESS' : 'FAILED');
    if (data1.isSuccess) {
        console.log(`  ID: ${data1.data.id}`);
        console.log(`  Title: ${data1.data.title}`);
        console.log(`  Status: ${data1.data.statusName}`);
    } else {
        console.log(`  Error: ${data1.message}`);
    }

    // Check complaint list
    console.log('\nChecking complaint list...');
    const response2 = await fetch('http://localhost:5000/api/complaints', {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    const data2 = await response2.json();
    console.log(`Total complaints: ${data2.data?.length || 0}`);

    if (data2.data && data2.data.length > 0) {
        console.log('\nFirst 3 complaints:');
        data2.data.slice(0, 3).forEach((c, i) => {
            console.log(`${i+1}. ${c.id} - ${c.title}`);
        });
    }
}

checkComplaint().catch(console.error);
