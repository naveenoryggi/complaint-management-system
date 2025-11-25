const http = require('http');
const fs = require('fs');
const token = fs.readFileSync('.working-token', 'utf8').trim();
const mapping = JSON.parse(fs.readFileSync('complaint-mapping.json', 'utf8'));
const priorities = JSON.parse(fs.readFileSync('priorities.json', 'utf8'));

// Get priority IDs
const lowPriority = priorities.find(p => p.name === 'Low')?.id;
const normalPriority = priorities.find(p => p.name === 'Normal')?.id;
const highPriority = priorities.find(p => p.name === 'High')?.id;
const criticalPriority = priorities.find(p => p.name === 'Critical')?.id;
const urgentPriority = priorities.find(p => p.name === 'Urgent')?.id;

function makeRequest(path, method, data) {
  return new Promise((resolve, reject) => {
    const postData = data ? JSON.stringify(data) : '';
    const options = {
      hostname: 'localhost',
      port: 5000,
      path: path,
      method: method,
      headers: {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json',
        'Content-Length': Buffer.byteLength(postData)
      }
    };

    const req = http.request(options, (res) => {
      let responseData = '';
      res.on('data', (chunk) => { responseData += chunk; });
      res.on('end', () => resolve({ status: res.statusCode, data: responseData }));
    });

    req.on('error', (e) => reject(e));
    if (postData) req.write(postData);
    req.end();
  });
}

async function createComplaint(complaintData) {
  console.log('\n' + '='.repeat(60));
  console.log('Creating Complaint:', complaintData.title);
  console.log('Category:', complaintData.categoryName);
  console.log('Priority:', complaintData.priorityName);
  console.log('='.repeat(60));

  try {
    const response = await makeRequest('/api/complaints', 'POST', complaintData);
    console.log('Status:', response.status);

    if (response.status === 200 || response.status === 201) {
      const result = JSON.parse(response.data);
      const complaint = result.data || result;
      console.log('SUCCESS! Complaint Created');
      console.log('  ID:', complaint.id);
      console.log('  Number:', complaint.complaintNumber);
      console.log('  Status:', complaint.statusName || 'N/A');
      return {
        success: true,
        id: complaint.id,
        number: complaint.complaintNumber,
        title: complaintData.title
      };
    } else {
      console.log('ERROR Response:', response.data.substring(0, 300));
      return {
        success: false,
        error: response.data,
        title: complaintData.title
      };
    }
  } catch (e) {
    console.log('EXCEPTION:', e.message);
    return {
      success: false,
      error: e.message,
      title: complaintData.title
    };
  }
}

async function run() {
  console.log('Starting complaint creation process...');
  console.log('Using complainant ID:', mapping.complainant);
  console.log('Using handler ID:', mapping.handler);

  const results = [];

  // Complaint 1: Low Priority - Attendance
  results.push(await createComplaint({
    title: 'Attendance marking issue',
    description: 'Unable to mark attendance in system. The check-in button is not responding when clicked.',
    categoryId: mapping.categories.attendance,
    categoryName: 'Attendance Issues',
    priorityId: lowPriority,
    priorityName: 'Low',
    complainantId: mapping.complainant,
    assignedToId: mapping.handler,
    source: 'Web Portal'
  }));

  await new Promise(r => setTimeout(r, 2000));

  // Complaint 2: Critical Priority - Billing
  results.push(await createComplaint({
    title: 'Payroll system down - URGENT',
    description: 'Cannot access payroll for salary processing. This is affecting payment processing for 500+ employees.',
    categoryId: mapping.categories.billing,
    categoryName: 'Billing Problems',
    priorityId: criticalPriority,
    priorityName: 'Critical',
    complainantId: mapping.complainant,
    assignedToId: mapping.handler,
    source: 'Email'
  }));

  await new Promise(r => setTimeout(r, 2000));

  // Complaint 3: High Priority - Technical
  results.push(await createComplaint({
    title: 'System crashes on login',
    description: 'Application crashes immediately after entering credentials. Cannot access any features.',
    categoryId: mapping.categories.technical,
    categoryName: 'Technical Issues',
    priorityId: highPriority,
    priorityName: 'High',
    complainantId: mapping.complainant,
    assignedToId: mapping.handler,
    source: 'Phone'
  }));

  await new Promise(r => setTimeout(r, 2000));

  // Complaint 4: Normal Priority - Service
  results.push(await createComplaint({
    title: 'Service delay in processing request',
    description: 'Request submitted 3 days ago but no response received yet.',
    categoryId: mapping.categories.service,
    categoryName: 'Service Delays',
    priorityId: normalPriority,
    priorityName: 'Normal',
    complainantId: mapping.complainant,
    assignedToId: mapping.handler,
    source: 'Web Portal'
  }));

  await new Promise(r => setTimeout(r, 2000));

  // Complaint 5: Urgent Priority - HRMS
  results.push(await createComplaint({
    title: 'HRMS system not accessible',
    description: 'Getting 503 error when trying to access HRMS portal. Need urgent access for leave approval.',
    categoryId: mapping.categories.hrms,
    categoryName: 'HRMS System',
    priorityId: urgentPriority,
    priorityName: 'Urgent',
    complainantId: mapping.complainant,
    assignedToId: mapping.handler,
    source: 'Web Portal'
  }));

  await new Promise(r => setTimeout(r, 2000));

  // Complaint 6: High Priority - Technical
  results.push(await createComplaint({
    title: 'Database connection timeout errors',
    description: 'Multiple users reporting timeout errors when querying large datasets.',
    categoryId: mapping.categories.technical,
    categoryName: 'Technical Issues',
    priorityId: highPriority,
    priorityName: 'High',
    complainantId: mapping.complainant,
    assignedToId: mapping.handler,
    source: 'Email'
  }));

  await new Promise(r => setTimeout(r, 2000));

  // Complaint 7: Normal Priority - Attendance
  results.push(await createComplaint({
    title: 'Incorrect attendance calculation',
    description: 'My attendance report shows 2 days less than actual attendance.',
    categoryId: mapping.categories.attendance,
    categoryName: 'Attendance Issues',
    priorityId: normalPriority,
    priorityName: 'Normal',
    complainantId: mapping.complainant,
    assignedToId: mapping.handler,
    source: 'Web Portal'
  }));

  await new Promise(r => setTimeout(r, 2000));

  // Complaint 8: Low Priority - Service
  results.push(await createComplaint({
    title: 'Request for user manual documentation',
    description: 'Need updated user manual for the new features released last month.',
    categoryId: mapping.categories.service,
    categoryName: 'Service Delays',
    priorityId: lowPriority,
    priorityName: 'Low',
    complainantId: mapping.complainant,
    assignedToId: mapping.handler,
    source: 'Phone'
  }));

  await new Promise(r => setTimeout(r, 2000));

  // Complaint 9: Critical Priority - HRMS
  results.push(await createComplaint({
    title: 'Salary credit failure for multiple employees',
    description: 'Automated salary transfer failed. 200+ employees did not receive their salaries.',
    categoryId: mapping.categories.hrms,
    categoryName: 'HRMS System',
    priorityId: criticalPriority,
    priorityName: 'Critical',
    complainantId: mapping.complainant,
    assignedToId: mapping.handler,
    source: 'Email'
  }));

  await new Promise(r => setTimeout(r, 2000));

  // Complaint 10: Urgent Priority - Billing
  results.push(await createComplaint({
    title: 'Invoice generation system showing errors',
    description: 'Cannot generate invoices for this month. Getting validation errors on all records.',
    categoryId: mapping.categories.billing,
    categoryName: 'Billing Problems',
    priorityId: urgentPriority,
    priorityName: 'Urgent',
    complainantId: mapping.complainant,
    assignedToId: mapping.handler,
    source: 'Web Portal'
  }));

  console.log('\n' + '='.repeat(60));
  console.log('=== FINAL SUMMARY ===');
  console.log('='.repeat(60));

  const successful = results.filter(r => r.success);
  const failed = results.filter(r => !r.success);

  console.log('\nTotal Complaints Created:', successful.length, '/ 10');
  console.log('Failed:', failed.length);

  if (successful.length > 0) {
    console.log('\n--- Successful Complaints ---');
    successful.forEach((r, i) => {
      console.log((i + 1) + '.', r.number, '-', r.title);
    });
  }

  if (failed.length > 0) {
    console.log('\n--- Failed Complaints ---');
    failed.forEach((r, i) => {
      console.log((i + 1) + '.', r.title);
      console.log('   Error:', r.error?.substring(0, 150));
    });
  }

  fs.writeFileSync('test-complaints-result.json', JSON.stringify(results, null, 2));
  console.log('\n=== Results saved to test-complaints-result.json ===\n');
}

run().catch(console.error);
