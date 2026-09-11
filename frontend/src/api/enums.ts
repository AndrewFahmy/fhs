export type Severity = "Minor" | "Major" | "Critical";

export type StatusFilter = "open" | "resolved" | "all";

export const severityOptions = [
    { value: "Minor", label: "Minor" },
    { value: "Major", label: "Major" },
    { value: "Critical", label: "Critical" },
];