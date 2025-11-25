// Inspect complaint data in detail
const fs = require('fs');

async function inspectComplaint() {
    // Login
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
    const companyId = loginData.data.companyId;

    console.log(`Admin Company ID: ${companyId}\n`);

    // Fetch the complaint
    const complaintId = 'b13be5c1-6e55-4a70-ac2d-9f5e935501e3';
    const response = await fetch(`http://localhost:5000/api/complaints/${complaintId}`, {
        headers: { 'Authorization': `Bearer ${token}` }
    });
    const data = await response.json();

    if (data.isSuccess) {
        const c = data.data;
        console.log('Complaint Details:');
        console.log(`  ID: ${c.id}`);
        console.log(`  Title: ${c.title}`);
        console.log(`  CompanyId: ${c.companyId}`);
        console.log(`  ComplainantId: ${c.complainantId}`);
        console.log(`  StatusMasterId: ${c.statusMasterId || c.statusId}`);
        console.log(`  PriorityMasterId: ${c.priorityMasterId || c.priorityId}`);
        console.log(`  CategoryId: ${c.categoryId}`);
        console.log(`\nMatches admin company: ${c.companyId === companyId}`);
    } else {
        console.log('Failed to fetch complaint');
    }
}

inspectComplaint().catch(console.error);
