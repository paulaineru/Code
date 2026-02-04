// Custom JavaScript for Swagger UI
window.onload = function() {
    // Add a custom header
    const header = document.createElement('div');
    header.className = 'custom-header';
    header.innerHTML = `
        <div style="background-color: #2c3e50; color: white; padding: 20px; text-align: center;">
            <h1 style="margin: 0;">Stegasi Property Management</h1>
            <p style="margin: 10px 0 0 0;">Property Management Service API Documentation</p>
        </div>
    `;
    document.body.insertBefore(header, document.body.firstChild);

    // Add a footer
    const footer = document.createElement('div');
    footer.className = 'custom-footer';
    footer.innerHTML = `
        <div style="background-color: #2c3e50; color: white; padding: 20px; text-align: center; margin-top: 20px;">
            <p style="margin: 0;">© ${new Date().getFullYear()} Stegasi Property Management. All rights reserved.</p>
        </div>
    `;
    document.body.appendChild(footer);

    // Add copy button to code blocks
    const codeBlocks = document.querySelectorAll('pre code');
    codeBlocks.forEach(block => {
        const button = document.createElement('button');
        button.className = 'copy-button';
        button.textContent = 'Copy';
        button.style.cssText = `
            position: absolute;
            top: 5px;
            right: 5px;
            padding: 5px 10px;
            background-color: #2c3e50;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        `;
        button.onclick = function() {
            navigator.clipboard.writeText(block.textContent);
            button.textContent = 'Copied!';
            setTimeout(() => {
                button.textContent = 'Copy';
            }, 2000);
        };
        block.parentElement.style.position = 'relative';
        block.parentElement.appendChild(button);
    });

    // Add tooltips to response codes
    const responseCodes = document.querySelectorAll('.response-col_status');
    responseCodes.forEach(code => {
        const status = code.textContent.trim();
        let tooltip = '';
        switch(status) {
            case '200':
                tooltip = 'Success';
                break;
            case '201':
                tooltip = 'Created';
                break;
            case '400':
                tooltip = 'Bad Request';
                break;
            case '401':
                tooltip = 'Unauthorized';
                break;
            case '403':
                tooltip = 'Forbidden';
                break;
            case '404':
                tooltip = 'Not Found';
                break;
            case '500':
                tooltip = 'Internal Server Error';
                break;
            default:
                tooltip = 'Unknown Status';
        }
        code.title = tooltip;
    });

    // Add a search box
    const searchBox = document.createElement('div');
    searchBox.className = 'search-box';
    searchBox.innerHTML = `
        <input type="text" 
               placeholder="Search endpoints..." 
               style="width: 100%; 
                      padding: 10px; 
                      margin: 10px 0; 
                      border: 1px solid #ddd; 
                      border-radius: 4px;">
    `;
    const searchInput = searchBox.querySelector('input');
    searchInput.onkeyup = function() {
        const searchText = this.value.toLowerCase();
        const endpoints = document.querySelectorAll('.opblock');
        endpoints.forEach(endpoint => {
            const text = endpoint.textContent.toLowerCase();
            endpoint.style.display = text.includes(searchText) ? 'block' : 'none';
        });
    };
    document.querySelector('.swagger-ui').insertBefore(searchBox, document.querySelector('.swagger-ui').firstChild);

    // Add version badge to title
    const title = document.querySelector('.swagger-ui .info .title');
    if (title) {
        const version = document.querySelector('.swagger-ui .info .version');
        if (version) {
            const badge = document.createElement('span');
            badge.className = 'version-badge';
            badge.textContent = version.textContent;
            title.appendChild(badge);
            version.style.display = 'none';
        }
    }

    // Add request duration to response section
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.addedNodes.length) {
                const responseSection = document.querySelector('.swagger-ui .responses-table');
                if (responseSection) {
                    const duration = document.querySelector('.swagger-ui .request-duration');
                    if (duration) {
                        const durationText = duration.textContent;
                        const durationElement = document.createElement('div');
                        durationElement.className = 'response-duration';
                        durationElement.textContent = `Request Duration: ${durationText}`;
                        durationElement.style.cssText = `
                            margin-top: 10px;
                            padding: 5px;
                            background: #f8f9fa;
                            border-radius: 3px;
                            font-size: 12px;
                            color: #2c3e50;
                        `;
                        responseSection.appendChild(durationElement);
                    }
                }
            }
        });
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true
    });

    // Add dark mode toggle
    const headerSwaggerUI = document.querySelector('.swagger-ui .topbar');
    if (headerSwaggerUI) {
        const darkModeToggle = document.createElement('button');
        darkModeToggle.className = 'dark-mode-toggle';
        darkModeToggle.textContent = '🌙';
        darkModeToggle.style.cssText = `
            position: absolute;
            right: 20px;
            top: 50%;
            transform: translateY(-50%);
            background: none;
            border: none;
            font-size: 20px;
            cursor: pointer;
            color: white;
        `;
        darkModeToggle.onclick = function() {
            document.body.classList.toggle('dark-mode');
            this.textContent = document.body.classList.contains('dark-mode') ? '☀️' : '🌙';
        };
        headerSwaggerUI.appendChild(darkModeToggle);
    }
}; 