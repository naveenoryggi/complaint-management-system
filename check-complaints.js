// Check complaints in database
const fs = require('fs');

async function checkComplaints() {
    const token = fs.readFileSync('.test-token-temp', 'utf8').trim();

    const response = await fetch('http://localhost:5000/api/complaints', {
        headers: { 'Authorization': `Bearer ${token}` }
    });

    const data = await response.json();

    console.log(`Total complaints: ${data.data?.length || 0}`);
    if (data.data && data.data.length > 0) {
        console.log('\nFirst 3 complaints:');
        data.data.slice(0, 3).forEach((c, i) => {
            console.log(`${i+1}. ${c.id} - ${c.title}`);
            console.log(`   Status: ${c.statusName}, Category: ${c.categoryName}`);
        });
    } else {
        console.log('No complaints found!');
    }
}

checkComplaints().catch(console.error);
