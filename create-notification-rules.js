const http = require('http');
const fs = require('fs');
const token = fs.readFileSync('.working-token', 'utf8').trim();
const mapping = JSON.parse(fs.readFileSync('notification-mapping.json', 'utf8'));

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

async function createRule(ruleName, eventTypeId, templateId, channel, recipientType, ccEmails = []) {
  const rule = {
    name: ruleName,
    eventTypeId: eventTypeId,
    templateId: templateId,
    channel: channel,
    recipientType: recipientType,
    isActive: true,
    priority: 100,
    delayMinutes: 0,
    sendOnlyOnce: false,
    conditions: null,
    specificEmails: ccEmails.length > 0 ? JSON.stringify(ccEmails) : null
  };

  console.log('\nCreating rule:', ruleName);
  console.log('Payload:', JSON.stringify(rule, null, 2));

  try {
    const response = await makeRequest('/api/event-communication-rules', 'POST', rule);
    console.log('Status:', response.status);
    if (response.status === 200 || response.status === 201) {
      const result = JSON.parse(response.data);
      console.log('SUCCESS! Rule ID:', result.data?.id || result.id);
      return { success: true, id: result.data?.id || result.id, rule: ruleName };
    } else {
      console.log('ERROR Response:', response.data);
      return { success: false, error: response.data, rule: ruleName };
    }
  } catch (e) {
    console.log('EXCEPTION:', e.message);
    return { success: false, error: e.message, rule: ruleName };
  }
}

async function run() {
  const results = [];

  // RecipientType enum: Complainant=0, AssignedHandler=1, Creator=2, SpecificUsers=3, SpecificRoles=4, SpecificEmails=5
  // CommunicationChannel enum: Email=0, SMS=1, WhatsApp=2

  // Rule 1: Complaint Created - Notify Complainant
  results.push(await createRule(
    'Complaint Created - Notify Complainant',
    mapping.events.created,
    mapping.templates.created,
    0, // Email
    0  // Complainant
  ));

  await new Promise(r => setTimeout(r, 1000));

  // Rule 2: Complaint Assigned - Notify Handler
  results.push(await createRule(
    'Complaint Assigned - Notify Handler',
    mapping.events.assigned,
    mapping.templates.assigned,
    0, // Email
    1  // AssignedHandler
  ));

  await new Promise(r => setTimeout(r, 1000));

  // Rule 3: Complaint Closed - Notify Complainant
  results.push(await createRule(
    'Complaint Closed - Notify Complainant',
    mapping.events.closed,
    mapping.templates.closed,
    0, // Email
    0  // Complainant
  ));

  await new Promise(r => setTimeout(r, 1000));

  // Rule 4: Complaint Closed - Notify Handler
  results.push(await createRule(
    'Complaint Closed - Notify Handler',
    mapping.events.closed,
    mapping.templates.closed,
    0, // Email
    1  // AssignedHandler
  ));

  await new Promise(r => setTimeout(r, 1000));

  // Rule 5: Complaint Escalated - Notify Handler with CC to support team
  results.push(await createRule(
    'Complaint Escalated - Notify Handler',
    mapping.events.escalated,
    mapping.templates.escalated,
    0, // Email
    1, // AssignedHandler
    ['support@oryggitech.com', 'marketing@oryggitech.com']
  ));

  console.log('\n=== SUMMARY ===');
  const successful = results.filter(r => r.success);
  const failed = results.filter(r => !r.success);

  console.log('Total Rules Created:', successful.length);
  console.log('Failed:', failed.length);

  if (successful.length > 0) {
    console.log('\nSuccessful Rules:');
    successful.forEach(r => console.log('  -', r.rule, '(ID:', r.id + ')'));
  }

  if (failed.length > 0) {
    console.log('\nFailed Rules:');
    failed.forEach(r => console.log('  -', r.rule, ':', r.error?.substring(0, 100)));
  }

  fs.writeFileSync('notification-rules-result.json', JSON.stringify(results, null, 2));
  console.log('\n=== Results saved to notification-rules-result.json ===');
}

run().catch(console.error);
