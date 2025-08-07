// Utility functions for UTC/Local time conversion
function convertUtcWithFormat(utcDateString, formatType = 'date') {
    if (!utcDateString) return '';

    const utcDate = new Date(utcDateString);
    const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

    let options = { timeZone };

    switch (formatType.toLowerCase()) {
        case 'date':
            options = {
                ...options,
                year: 'numeric',
                month: 'short',
                day: 'numeric'
            };
            return utcDate.toLocaleDateString(undefined, options);

        case 'date-long':
            options = {
                ...options,
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            };
            return utcDate.toLocaleDateString(undefined, options);

        case 'time':
            options = {
                ...options,
                hour: '2-digit',
                minute: '2-digit'
            };
            return utcDate.toLocaleTimeString(undefined, options);

        case 'time-seconds':
            options = {
                ...options,
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit'
            };
            return utcDate.toLocaleTimeString(undefined, options);

        case 'datetime':
            options = {
                ...options,
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            };
            return utcDate.toLocaleString(undefined, options);

        case 'datetime-long':
            options = {
                ...options,
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            };
            return utcDate.toLocaleString(undefined, options);

        case 'relative':
            return formatRelativeTime(utcDateString);

        default:
            // Default to date format
            options = {
                ...options,
                year: 'numeric',
                month: 'short',
                day: 'numeric'
            };
            return utcDate.toLocaleDateString(undefined, options);
    }
}

function formatRelativeTime(utcDateString) {
    if (!utcDateString) return '';

    const utcDate = new Date(utcDateString);
    const now = new Date();
    const diffInMs = utcDate - now;
    const diffInDays = Math.ceil(diffInMs / (1000 * 60 * 60 * 24));

    if (diffInDays < 0) {
        return `${Math.abs(diffInDays)} days ago`;
    } else if (diffInDays === 0) {
        return 'Today';
    } else if (diffInDays === 1) {
        return 'Tomorrow';
    } else {
        return `In ${diffInDays} days`;
    }
}

// Global helper function for manual date conversion
window.convertUTCDate = function (utcDateString, format = 'date') {
    return convertUtcWithFormat(utcDateString, format);
};

// Convert all UTC dates to local time on page load
document.addEventListener('DOMContentLoaded', function () {
    const utcDateElements = document.querySelectorAll('.utc-date');

    utcDateElements.forEach(element => {
        const utcDate = element.getAttribute('data-utc');
        const format = element.getAttribute('data-utc-format') || 'date';

        if (utcDate) {
            // Update the display with local time using specified format
            element.textContent = convertUtcWithFormat(utcDate, format);

            // Add tooltip with full date and time (always show complete info in tooltip)
            element.title = convertUtcWithFormat(utcDate, 'datetime-long');
            element.setAttribute('data-bs-toggle', 'tooltip');
        }
    });
});
