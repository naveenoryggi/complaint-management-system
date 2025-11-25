// Create Test Complaint for E2E Tests
// Node 22+ has native fetch - no import needed
const baseUrl = 'http://localhost:5000/api';

async function createTestComplaint() {
    console.log('Creating test complaint...\n');

    // Step 1: Login as admin
    console.log('[1/4] Logging in as admin...');
    const loginResponse = await fetch(`${baseUrl}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            email: 'admin@complaintmanagement.com',
            password: 'Admin@123'
        })
    });
    const loginData = await loginResponse.json();
    const token = loginData.data.token;
    console.log('  ✓ Logged in\n');

    const headers = {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };

    // Step 2: Get master data
    console.log('[2/4] Getting master data...');
    const [categoriesRes, prioritiesRes, statusesRes] = await Promise.all([
        fetch(`${baseUrl}/categories`, { headers }),
        fetch(`${baseUrl}/complaintprioritymaster`, { headers }),
        fetch(`${baseUrl}/complaintstatusmaster`, { headers })
    ]);

    const categories = await categoriesRes.json();
    const priorities = await prioritiesRes.json();
    const statuses = await statusesRes.json();

    const categoryId = categories.data[0]?.id;
    const priorityId = priorities.data[0]?.id;
    const statusId = statuses.data.find(s => s.name?.includes('Open') || s.name?.includes('New'))?.id || statuses.data[0]?.id;

    console.log(`  Category ID: ${categoryId}`);
    console.log(`  Priority ID: ${priorityId}`);
    console.log(`  Status ID: ${statusId}\n`);

    // Step 3: Create complaint
    console.log('[3/4] Creating test complaint...');
    const complaintData = {
        title: 'E2E Test Complaint - System Performance Issue',
        description: 'This is a test complaint created for E2E testing purposes. The system is experiencing slow response times during peak hours. Users report delays of 5-10 seconds when loading pages.',
        categoryId: categoryId,
        priorityId: priorityId,
        statusId: statusId,
        complainantId: '7d21390e-d60e-4938-a798-8f571d5110b7'  // Our test complainant
    };

    const createResponse = await fetch(`${baseUrl}/complaints`, {
        method: 'POST',
        headers: headers,
        body: JSON.stringify(complaintData)
    });

    const createResult = await createResponse.json();

    if (createResult.isSuccess) {
        const complaintId = createResult.data.id;
        console.log(`  ✓ Complaint created: ${complaintId}\n`);

        // Step 4: Verify complaint exists
        console.log('[4/4] Verifying complaint...');
        const verifyResponse = await fetch(`${baseUrl}/complaints/${complaintId}`, { headers });
        const verifyData = await verifyResponse.json();

        if (verifyData.isSuccess) {
            console.log('  ✓ Complaint verified\n');
            console.log('=====================================');
            console.log('SUCCESS! Test complaint created');
            console.log('=====================================');
            console.log(`Complaint ID: ${complaintId}`);
            console.log(`Title: ${verifyData.data.title}`);
            console.log(`Status: ${verifyData.data.statusName}`);
            console.log(`Category: ${verifyData.data.categoryName}`);
            console.log(`\nYou can now run E2E tests - the detail view test should pass!`);
        } else {
            console.log('  ✗ Could not verify complaint');
        }
    } else {
        console.log('  ✗ Failed to create complaint');
        console.log(`  Error: ${createResult.message}`);
        console.log(`  Details: ${JSON.stringify(createResult.errors)}`);
    }
}

createTestComplaint().catch(console.error);
