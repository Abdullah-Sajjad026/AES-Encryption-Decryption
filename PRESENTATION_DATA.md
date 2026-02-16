# AES Encryption Project - Presentation Data

## Introduction/Background

- **Overview**: In today's digital age, data security is paramount. The increasing volume of sensitive information exchanged online necessitates robust encryption mechanisms to protect data from unauthorized access and cyber threats.
- **Technology**: This project leverages the Advanced Encryption Standard (AES) in Galois/Counter Mode (GCM), a widely adopted symmetric encryption algorithm known for its speed and security.
- **Context**: The application provides a seamless web-based interface for users to encrypt and decrypt files, ensuring confidentiality and integrity. It integrates with cloud storage (Google Drive) to facilitate secure file sharing and backup.
- **Relevance**: Addressing the critical need for accessible, user-friendly encryption tools for individuals and organizations handling sensitive documents.

## Problem Statement

- **Insecure File Handling**: Traditional file sharing methods (email, messaging apps) often transmit data in plaintext or with weak encryption, exposing it to interception.
- **Key Management Complexity**: Many existing tools require users to manage encryption keys manually, leading to poor practices like key reuse or loss.
- **Usability Gap**: Complex cryptographic software often alienates non-technical users, discouraging the adoption of secure practices.
- **Legacy Vulnerabilities**: Older systems may use insecure modes (ECB, CBC without proper padding) or fail to implement authenticated encryption, leaving data vulnerable to tampering (e.g., bit-flipping attacks) and implementation flaws (e.g., Zip Slip).

## Project Scope

- **Web-Based Application**: A full-stack web application accessible from any browser, eliminating the need for local software installation.
- **Secure File Encryption/Decryption**: Implementation of AES-GCM (128/192/256-bit) for authenticated encryption of user-uploaded files.
- **Automated Key Management**: Secure generation, storage, and retrieval of encryption keys and initialization vectors (IVs) per file, abstracting complexity from the user.
- **Cloud Integration**: Seamless integration with Google Drive API v3 for direct upload of encrypted archives.
- **User Authentication**: Secure user registration and login system to protect access to encryption services.
- **Cross-Platform Compatibility**: Backend built on .NET 8 (cross-platform) and Frontend on React, ensuring broad accessibility.

## Project Type

- **Applied Project**: This project focuses on the practical application of established cryptographic standards (NIST FIPS 197) and modern web development frameworks to solve real-world security problems. It demonstrates the integration of security best practices (secure coding, input validation, secure storage) into a functional software product.

## Functional Requirements

1.  **User Management**:
    -   Users must be able to register with a username and password.
    -   Users must be able to log in securely to access the dashboard.
    -   System must handle authentication state and session management (JWT/Cookies).

2.  **File Operations**:
    -   Users can upload files of various formats for encryption.
    -   Users can select the encryption key size (128, 192, or 256 bits).
    -   System must encrypt files using AES-GCM and package them (Zip) with necessary metadata (encrypted keys/IV references).
    -   Users can upload encrypted zip files for decryption.
    -   System must validate file integrity and authenticity (GCM tag check) before decryption.

3.  **Cloud Integration**:
    -   Users can authorize the application to access their Google Drive (OAuth 2.0).
    -   Users can choose to automatically upload encrypted files to their Google Drive.
    -   Users can specify email addresses to share the uploaded file with (Google Drive sharing).

4.  **Security**:
    -   Prevention of Zip Slip vulnerabilities during file extraction.
    -   Secure storage of encryption metadata (keys, IVs) in a relational database.
    -   Secure handling of user credentials (hashing/salting via ASP.NET Identity).

## Tools/Domain

-   **Backend**: .NET 8 (C#), ASP.NET Core Web API.
-   **Frontend**: React (TypeScript), Vite, Tailwind CSS.
-   **Database**: PostgreSQL (Entity Framework Core).
-   **Cryptography**: `System.Security.Cryptography.AesGcm` (AES-GCM).
-   **Authentication**: ASP.NET Core Identity.
-   **Cloud API**: Google Drive API v3 (Google.Apis.Drive.v3).
-   **Containerization**: Docker (optional/recommended).
-   **Testing**: xUnit, Moq.

## Methodology Diagram (Conceptual Flow)

1.  **User Action**: User logs in and uploads a file via React Frontend.
2.  **API Request**: Frontend sends file + options (Key Size, Drive Upload) to .NET Backend API.
3.  **Encryption Service**:
    -   Generates unique random Key and IV.
    -   Encrypts file content using AES-GCM.
    -   Stores Key, IV, and Tag Length in PostgreSQL Database (linked by UUID).
4.  **File Packaging**:
    -   Backend creates a Zip archive containing the Encrypted File and a Metadata ID file.
    -   (Zip Slip protection applied during any extraction).
5.  **Cloud Upload (Optional)**:
    -   Backend uses stored User OAuth Token to upload the Zip to Google Drive.
    -   Applies sharing permissions if requested.
6.  **Response**: Backend returns the Zip file stream (download) or success message to Frontend.

## Business Model (Hypothetical)

-   **Freemium Model**:
    -   **Free Tier**: Basic encryption (128-bit), local download only, file size limit (e.g., 50MB).
    -   **Premium Tier**: Advanced encryption (256-bit), Google Drive integration, unlimited file size, priority support.
-   **Target Audience**:
    -   **Individuals**: Privacy-conscious users, freelancers sharing sensitive contracts.
    -   **SMEs**: Small businesses needing secure document transfer without expensive enterprise suites.
    -   **Educational**: Students/Researchers sharing data securely.
