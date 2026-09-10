import { useState } from "react";
import Icon, { type IconName } from "@/components/common/icon";
import { Menu, MenuButton } from "@/components/navigation/menu";

type Theme = "system" | "light" | "dark";

const storageKey = "fhs:theme";

const themeIcons: Record<Theme, IconName> = {
    system: "monitor",
    light: "sun",
    dark: "moon",
};

const themeLabels: Record<Theme, string> = {
    system: "System",
    light: "Light",
    dark: "Dark",
};

function readStoredTheme(): Theme {
    const stored = localStorage.getItem(storageKey);

    return stored === "light" || stored === "dark" ? stored : "system";
}

function ThemeToggle() {
    const [theme, setTheme] = useState<Theme>(readStoredTheme);

    function choose(next: Theme) {
        setTheme(next);

        if (next === "system") {
            localStorage.removeItem(storageKey);
            document.documentElement.removeAttribute("data-theme");
            return;
        }

        localStorage.setItem(storageKey, next);
        document.documentElement.setAttribute("data-theme", next);
    }

    return (
        <Menu
            align="end"
            chevron={false}
            triggerVariant="subtle"
            triggerSize="icon"
            label={
                <>
                    <Icon name={themeIcons[theme]} className="size-5" />
                    <span className="sr-only">Appearance</span>
                </>
            }
        >
            {(["system", "light", "dark"] as const).map((option) => (
                <MenuButton
                    key={option}
                    active={theme === option}
                    onClick={() => choose(option)}
                >
                    {themeLabels[option]}
                </MenuButton>
            ))}
        </Menu>
    );
}

export default ThemeToggle;
