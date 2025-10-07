# Py-Prox
A simple **Python-based HTTP/HTTPS proxy server** with a **client test utility**.  
The proxy supports **CONNECT tunneling** for HTTPS requests and relays normal HTTP requests.  

## Features

* Accepts client connections and forwards HTTP(S) traffic  
* Supports **HTTPS CONNECT tunneling**  
* Handles **normal HTTP requests** with host header parsing  
* Bidirectional data forwarding using **threads**  
* Test client using the **Requests library** with and without proxy  
* Error handling for timeouts, socket issues, and invalid requests  
* Console logging for debugging connections and errors  

## How It Works

1. The **server** listens on `127.0.0.1:8888` for incoming connections.  
2. When a client sends an **HTTP request**:  
   * The proxy parses the `Host` header to find the destination server.  
   * It forwards the request and relays responses back to the client.  
3. When a client sends an **HTTPS CONNECT request**:  
   * The proxy establishes a tunnel between the client and the target server.  
   * Encrypted traffic flows transparently through the proxy.  
4. Two threads are created for each connection:  
   * **Client → Server** data forwarding  
   * **Server → Client** data forwarding  
5. The **client script** demonstrates usage:  
   * You can choose to send the test request **with** or **without** proxy.  
   * It fetches IP/location info from [ipinfo.io](https://ipinfo.io).    

## Getting Started

### Prerequisites

* [Python 3.13.7 (or newer)](https://www.python.org/downloads)

## License

This project is licensed under the MIT License in its parent directory.
