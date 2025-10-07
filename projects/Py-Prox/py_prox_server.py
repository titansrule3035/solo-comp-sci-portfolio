import socket
import threading

# Proxy server configuration
proxy_host = '127.0.0.1'   # Localhost (runs only on the same machine)
proxy_port = 8888          # Port where the proxy listens

def handle_client(client_socket):
    """
    Handles each client that connects to the proxy.
    Determines whether the client is making an HTTPS CONNECT request
    or a normal HTTP request, and forwards data accordingly.
    """
    try:
        # Receive the initial request from the client (up to 4KB)
        request = client_socket.recv(4096).decode()
        if not request:
            print("[INFO] Empty request received. Closing connection.")
            client_socket.close()
            return

        # Parse the request line (e.g., "GET http://... HTTP/1.1" or "CONNECT host:443 HTTP/1.1")
        first_line = request.split("\r\n")[0]
        method, url, protocol = first_line.split()

        # Handle HTTPS requests (CONNECT establishes a tunnel)
        if method == "CONNECT":
            target_host, target_port = url.split(":")
            target_port = int(target_port)

            # Connect to the actual target server
            server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            server_socket.connect((target_host, target_port))
            print(f"[INFO] Established tunnel to {target_host}:{target_port}")

            # Acknowledge success to the client
            client_socket.send(b"HTTP/1.1 200 Connection Established\r\n\r\n")

            # Start relaying encrypted traffic both ways
            relay_data_bidirectional(client_socket, server_socket)
            return

        # Handle normal HTTP requests
        else:
            headers = request.split("\r\n")

            # Extract Host header to know where to forward
            host_header = [header for header in headers if header.lower().startswith("host:")][0]
            target_host, _, target_port = host_header.partition(":")[2].strip().partition(":")
            target_port = int(target_port) if target_port else 80  # Default to port 80 for HTTP

            # Connect to the target host
            server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            server_socket.connect((target_host, target_port))
            print(f"[INFO] Forwarding HTTP request to {target_host}:{target_port}")

            # Send the full HTTP request
            server_socket.send(request.encode())

            # Relay traffic both ways
            relay_data_bidirectional(client_socket, server_socket)

    except Exception as e:
        print(f"[ERROR] Exception in client handler: {e}")
    finally:
        # Always close the client socket when done
        client_socket.close()

def relay_data_bidirectional(client_socket, server_socket):
    """
    Forwards data between the client and server sockets in both directions.
    Uses two threads to allow simultaneous send/receive.
    """
    def forward_data(source, destination):
        try:
            while True:
                # Read from source
                data = source.recv(4096)
                if not data:  # Connection closed
                    break
                # Forward to destination
                destination.sendall(data)
        except (socket.error, socket.timeout) as e:
            print(f"[INFO] Socket error or timeout: {e}")
        except Exception as e:
            print(f"[ERROR] Exception during data forwarding: {e}")
        finally:
            print(f"[DEBUG] Exiting thread for {source}")

    # Threads for client→server and server→client data
    client_to_server = threading.Thread(target=forward_data, args=(client_socket, server_socket))
    server_to_client = threading.Thread(target=forward_data, args=(server_socket, client_socket))

    client_to_server.start()
    server_to_client.start()

    # Wait until both finish
    client_to_server.join()
    server_to_client.join()

    # Clean up sockets
    try:
        client_socket.shutdown(socket.SHUT_RDWR)
    except Exception as e:
        print(f"[INFO] Error shutting down client socket: {e}")
    finally:
        client_socket.close()

    try:
        server_socket.shutdown(socket.SHUT_RDWR)
    except Exception as e:
        print(f"[INFO] Error shutting down server socket: {e}")
    finally:
        server_socket.close()

def start_proxy():
    """Starts the proxy server and listens for new client connections."""
    print("[INFO] Starting proxy server...")
    try:
        proxy = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        proxy.bind((proxy_host, proxy_port))
        proxy.listen(5)  # Allow up to 5 queued connections
        print(f"[INFO] Proxy server listening on {proxy_host}:{proxy_port}")

        while True:
            # Accept client connection
            client_socket, addr = proxy.accept()
            print(f"[INFO] Accepted connection from {addr[0]}:{addr[1]}")

            # Handle each client in a separate thread
            client_handler = threading.Thread(target=handle_client, args=(client_socket,))
            client_handler.start()
    except Exception as e:
        print(f"[ERROR] Exception in proxy server: {e}")
    finally:
        proxy.close()
        print("[INFO] Proxy server shutting down.")

if __name__ == "__main__":
    start_proxy()
