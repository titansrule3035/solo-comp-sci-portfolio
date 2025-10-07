import requests

# Proxy configuration
proxy = '127.0.0.1:8888'
proxies = {
    'https': proxy  # Only routing HTTPS requests through the proxy
}

print("Use proxy from URL in file? (Y/N)")
intake = input().strip().lower()

# Timeout tuple: (connection timeout, read timeout)
timeout = (5, 10)  # Connect within 5s, read within 10s

try:
    if intake == "y":
        # Test request using the proxy
        print(f"Testing with proxy {proxy}...")
        response = requests.get("https://ipinfo.io/json", proxies=proxies, timeout=timeout)
        print("Connection established. Waiting for data...")
    elif intake == "n":
        # Test request without proxy
        print("Testing without proxy...")
        response = requests.get("https://ipinfo.io/json", timeout=timeout)
        print("Connection established. Waiting for data...")
    else:
        print("Invalid input. Exiting.")
        exit()

    # Pretty-print response information from ipinfo.io
    data = response.json()
    print('IP: ' + data['ip'])
    print('Hostname: ' + data['hostname'])
    print('City: ' + data['city'])
    print('Region: ' + data['region'])
    print('Country: ' + data['country'])
    print('Location: ' + data['loc'])
    print('Organization: ' + data['org'])
    print('Postal/ZIP Code: ' + data['postal'])
    print('Timezone: ' + data['timezone'])

# Handle common networking exceptions
except requests.exceptions.ConnectTimeout:
    print("Connection timeout: Unable to establish a connection to the server.")
except requests.exceptions.ReadTimeout:
    print("Read timeout: Connection established, but no data received within the timeout period.")
except requests.exceptions.ProxyError:
    print("Proxy error occurred: The proxy may be invalid or unsupported.")
except requests.exceptions.RequestException as e:
    print(f"An error occurred: {e}")
