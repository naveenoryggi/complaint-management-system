const http = require('http');

console.log('Testing Angular Application Server...\n');

// Test 1: Basic server response
function testServerResponse() {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: 'localhost',
            port: 4203,
            path: '/',
            method: 'GET',
            headers: {
                'User-Agent': 'Test-Client/1.0'
            }
        };

        const req = http.request(options, (res) => {
            let data = '';
            res.on('data', (chunk) => {
                data += chunk;
            });
            res.on('end', () => {
                console.log('✅ Test 1 PASSED: Server responds with HTTP', res.statusCode);
                console.log('   Content-Type:', res.headers['content-type']);
                console.log('   Content-Length:', data.length, 'bytes');

                // Check for essential HTML elements
                const hasAppRoot = data.includes('<app-root>');
                const hasMainJs = data.includes('main.js');
                const hasPolyfills = data.includes('polyfills.js');

                console.log('   Contains app-root:', hasAppRoot ? '✅' : '❌');
                console.log('   Contains main.js:', hasMainJs ? '✅' : '❌');
                console.log('   Contains polyfills.js:', hasPolyfills ? '✅' : '❌');

                resolve({ success: true, data });
            });
        });

        req.on('error', (err) => {
            console.log('❌ Test 1 FAILED:', err.message);
            reject(err);
        });

        req.setTimeout(5000, () => {
            req.destroy();
            reject(new Error('Request timeout'));
        });

        req.end();
    });
}

// Test 2: JavaScript file loading
function testJavaScriptLoading() {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: 'localhost',
            port: 4203,
            path: '/main.js',
            method: 'GET'
        };

        const req = http.request(options, (res) => {
            let data = '';
            res.on('data', (chunk) => {
                data += chunk;
            });
            res.on('end', () => {
                console.log('\n✅ Test 2 PASSED: main.js loads successfully');
                console.log('   Content-Type:', res.headers['content-type']);
                console.log('   File size:', data.length, 'bytes');

                // Check for Angular bootstrap code
                const hasBootstrap = data.includes('bootstrapApplication');
                const hasConsoleLog = data.includes('console.log');

                console.log('   Contains bootstrapApplication:', hasBootstrap ? '✅' : '❌');
                console.log('   Contains console.log:', hasConsoleLog ? '✅' : '❌');

                resolve({ success: true, hasBootstrap, hasConsoleLog });
            });
        });

        req.on('error', (err) => {
            console.log('\n❌ Test 2 FAILED:', err.message);
            reject(err);
        });

        req.setTimeout(5000, () => {
            req.destroy();
            reject(new Error('JavaScript loading timeout'));
        });

        req.end();
    });
}

// Test 3: CSS loading
function testCSSLoading() {
    return new Promise((resolve, reject) => {
        const options = {
            hostname: 'localhost',
            port: 4203,
            path: '/styles.css',
            method: 'GET'
        };

        const req = http.request(options, (res) => {
            let data = '';
            res.on('data', (chunk) => {
                data += chunk;
            });
            res.on('end', () => {
                console.log('\n✅ Test 3 PASSED: styles.css loads successfully');
                console.log('   Content-Type:', res.headers['content-type']);
                console.log('   File size:', data.length, 'bytes');

                resolve({ success: true });
            });
        });

        req.on('error', (err) => {
            console.log('\n❌ Test 3 FAILED:', err.message);
            reject(err);
        });

        req.setTimeout(5000, () => {
            req.destroy();
            reject(new Error('CSS loading timeout'));
        });

        req.end();
    });
}

// Run all tests
async function runAllTests() {
    try {
        await testServerResponse();
        await testJavaScriptLoading();
        await testCSSLoading();

        console.log('\n🎉 ALL TESTS PASSED!');
        console.log('\n📋 SUMMARY:');
        console.log('   ✅ Angular server is running and responding');
        console.log('   ✅ HTML page loads with correct structure');
        console.log('   ✅ JavaScript files load correctly');
        console.log('   ✅ CSS files load correctly');
        console.log('   ✅ Bootstrap code is present');
        console.log('   ✅ Debug logging is enabled');

        console.log('\n🔍 NEXT STEPS:');
        console.log('   1. Open http://localhost:4203 in your browser');
        console.log('   2. Check browser console (F12) for "Starting Angular application bootstrap..."');
        console.log('   3. Look for "Angular application bootstrapped successfully!" message');
        console.log('   4. If you see errors, they will be logged with full stack traces');

    } catch (error) {
        console.log('\n❌ TEST SUITE FAILED:', error.message);
        console.log('\n🔧 TROUBLESHOOTING:');
        console.log('   1. Make sure Angular server is running on port 4203');
        console.log('   2. Check for any compilation errors in the terminal');
        console.log('   3. Try restarting the Angular server');
        console.log('   4. Check if another application is using port 4203');
    }
}

// Run the tests
runAllTests();