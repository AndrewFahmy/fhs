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

export function stringTemplateFormat(template: string, ...args: string[]): string {
    return template.replace(
        /{(\d+)}/g,
        (match, index: string) => args[Number(index)] ?? match,
    );
}

export function formatWhen(value: string): string {
    const formatted = formatTimestamp(value);

    return formatted.includes(" ")
        ? `on ${formatted.replace(" ", " at ")}`
        : `today at ${formatted}`;
}

/** Grouped thousands, so a six-figure total stays readable: 1,204,553. */
export function formatCount(value: number): string {
    return value.toLocaleString();
}