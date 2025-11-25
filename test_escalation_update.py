import requests
import json
import sys

BASE_URL = "http://localhost:5058"

def login():
    """Login and get JWT token"""
    print("=== Step 1: Logging in ===")
    login_data = {
        "email": "admin@complaintmanagement.com",
        "password": "Admin@123"
    }

    response = requests.post(f"{BASE_URL}/api/auth/login", json=login_data)
    if response.status_code == 200:
        token = response.json()["data"]["token"]
        print(f"✓ Logged in successfully")
        return token
    else:
        print(f"✗ Login failed: {response.status_code}")
        print(response.text)
        return None

def get_matrices(token):
    """Get all escalation matrices"""
    print("\n=== Step 2: Getting escalation matrices ===")
    headers = {"Authorization": f"Bearer {token}"}

    response = requests.get(f"{BASE_URL}/api/escalation/matrices", headers=headers)
    if response.status_code == 200:
        matrices = response.json()["data"]
        print(f"✓ Found {len(matrices)} matrices")
        if len(matrices) > 0:
            matrix = matrices[0]
            print(f"  Matrix ID: {matrix['id']}")
            print(f"  Name: {matrix['name']}")
            if len(matrix['escalationLevels']) > 0:
                level = matrix['escalationLevels'][0]
                print(f"  First Level - TriggerAfterValue: {level.get('triggerAfterValue', 'N/A')}, TriggerTimeUnit: {level.get('triggerTimeUnit', 'N/A')}, TriggerAfterHours: {level.get('triggerAfterHours', 'N/A')}")
            return matrix
        else:
            print("✗ No matrices found in database")
            return None
    else:
        print(f"✗ Failed to get matrices: {response.status_code}")
        print(response.text)
        return None

def update_matrix(token, matrix):
    """Update escalation matrix with 24 hours trigger value"""
    print("\n=== Step 3: Updating matrix with 24 hours ===")
    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}

    # Prepare update payload
    update_data = {
        "name": matrix["name"],
        "description": matrix.get("description", ""),
        "categoryId": matrix.get("categoryId"),
        "branchId": matrix.get("branchId"),
        "departmentId": matrix.get("departmentId"),
        "isActive": matrix.get("isActive", True),
        "priority": matrix.get("priority", 0),
        "enableAutoEscalation": matrix.get("enableAutoEscalation", True),
        "sendEmailNotifications": matrix.get("sendEmailNotifications", True),
        "escalationLevels": [
            {
                "level": 1,
                "name": "Level 1 Test",
                "description": "Test level",
                "triggerAfterValue": 24,
                "triggerTimeUnit": 1,  # Hours = 1
                "triggerAfterHours": 0,
                "assignmentStrategy": 0,
                "sendNotification": True,
                "notifyPreviousHandler": True
            }
        ]
    }

    print(f"  Sending: triggerAfterValue=24, triggerTimeUnit=1 (Hours)")
    print(f"  Payload: {json.dumps(update_data, indent=2)}")

    response = requests.put(f"{BASE_URL}/api/escalation/matrices/{matrix['id']}",
                           headers=headers, json=update_data)

    if response.status_code == 200:
        result = response.json()
        print(f"✓ Update successful")
        if result.get("data") and len(result["data"]["escalationLevels"]) > 0:
            level = result["data"]["escalationLevels"][0]
            print(f"  Response - TriggerAfterValue: {level.get('triggerAfterValue', 'N/A')}, TriggerTimeUnit: {level.get('triggerTimeUnit', 'N/A')}, TriggerAfterHours: {level.get('triggerAfterHours', 'N/A')}")
            return result["data"]
        return result.get("data")
    else:
        print(f"✗ Update failed: {response.status_code}")
        print(response.text)
        return None

def verify_matrix(token, matrix_id):
    """Verify the matrix was updated correctly"""
    print("\n=== Step 4: Verifying update ===")
    headers = {"Authorization": f"Bearer {token}"}

    response = requests.get(f"{BASE_URL}/api/escalation/matrices/{matrix_id}", headers=headers)
    if response.status_code == 200:
        result = response.json()
        print(f"✓ Retrieved updated matrix")
        if result.get("data") and len(result["data"]["escalationLevels"]) > 0:
            level = result["data"]["escalationLevels"][0]
            trigger_value = level.get('triggerAfterValue', 0)
            trigger_unit = level.get('triggerTimeUnit', 0)
            trigger_hours = level.get('triggerAfterHours', 0)

            print(f"  TriggerAfterValue: {trigger_value}")
            print(f"  TriggerTimeUnit: {trigger_unit}")
            print(f"  TriggerAfterHours: {trigger_hours}")

            if trigger_value == 24 and trigger_unit == 1 and trigger_hours == 24:
                print(f"\n✓✓✓ SUCCESS! All values are correct!")
                return True
            else:
                print(f"\n✗✗✗ FAILURE! Values are incorrect!")
                print(f"  Expected: triggerAfterValue=24, triggerTimeUnit=1, triggerAfterHours=24")
                print(f"  Got: triggerAfterValue={trigger_value}, triggerTimeUnit={trigger_unit}, triggerAfterHours={trigger_hours}")
                return False
        return False
    else:
        print(f"✗ Failed to verify: {response.status_code}")
        print(response.text)
        return False

def main():
    print("="*60)
    print("AUTOMATED ESCALATION MATRIX UPDATE TEST")
    print("="*60)

    # Step 1: Login
    token = login()
    if not token:
        sys.exit(1)

    # Step 2: Get matrices
    matrix = get_matrices(token)
    if not matrix:
        sys.exit(1)

    # Step 3: Update matrix
    updated = update_matrix(token, matrix)
    if not updated:
        sys.exit(1)

    # Step 4: Verify
    success = verify_matrix(token, matrix['id'])

    print("\n" + "="*60)
    if success:
        print("TEST PASSED - Data is being saved and retrieved correctly!")
    else:
        print("TEST FAILED - Check the backend logs for detailed information")
    print("="*60)

    sys.exit(0 if success else 1)

if __name__ == "__main__":
    main()
