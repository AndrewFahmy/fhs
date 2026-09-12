import { NavLink, useLocation, type NavLinkRenderProps } from "react-router";
import {
    actionBarPaths,
    isNavGroup,
    navigation,
    type NavEntry,
} from "@/routes/navigation";
import { Menu, MenuItem } from "@/components/navigation/menu";

const tabClass =
    "items-center justify-center text-[11px] font-bold tracking-wide uppercase transition-colors";

function tabLinkClass({ isActive }: NavLinkRenderProps) {
    return `flex flex-1 ${tabClass} ${isActive ? "text-ink" : "text-ink-muted"}`;
}

function mapTab(entry: NavEntry, pathname: string) {
    if (!isNavGroup(entry)) {
        return (
            <NavLink key={entry.to} to={entry.to} className={tabLinkClass}>
                {entry.label}
            </NavLink>
        );
    }

    const active = entry.items.some((item) => pathname.startsWith(item.to));

    return (
        <Menu
            key={entry.label}
            align="end"
            placement="top"
            chevron={false}
            className="flex flex-1"
            triggerClassName={`flex-1 ${tabClass} ${active ? "text-ink" : "text-ink-muted"}`}
            label={entry.label}
        >
            {entry.items.map((item) => (
                <MenuItem key={item.to} to={item.to}>
                    {item.label}
                </MenuItem>
            ))}
        </Menu>
    );
}

function BottomTabs() {
    const location = useLocation();

    if (actionBarPaths.includes(location.pathname)) {
        return null;
    }

    return (
        <nav
            aria-label="Primary"
            className="fixed inset-x-0 bottom-0 z-20 border-t border-border-subtle bg-surface pb-[env(safe-area-inset-bottom)] md:hidden"
        >
            <div className="flex h-14">
                {navigation.map((entry) => mapTab(entry, location.pathname))}
            </div>
        </nav>
    );
}

export default BottomTabs;
