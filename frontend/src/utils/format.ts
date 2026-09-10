function pad(value: number): string {
    return String(value).padStart(2, "0");
}

export function formatTimestamp(value: string): string {
    const at = new Date(value);
    const now = new Date();
    const time = `${pad(at.getHours())}:${pad(at.getMinutes())}`;

    const sameDay =
        at.getFullYear() === now.getFullYear() &&
        at.getMonth() === now.getMonth() &&
        at.getDate() === now.getDate();

    if (sameDay) {
        return time;
    }

    return `${at.getFullYear()}-${pad(at.getMonth() + 1)}-${pad(at.getDate())} ${time}`;
}

export function fullTimestamp(value: string): string {
    return new Date(value).toLocaleString();
}


// function stringTemplateFormat(template: string, ...args: any[]): string {
//     return template.replace(/{(\d+)}/g, (match, index) => {
//         return typeof args[index] !== 'undefined' ? args[index] : match;
//     });
// }