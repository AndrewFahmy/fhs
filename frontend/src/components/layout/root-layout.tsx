import { useAuth } from "react-oidc-context";
import {
    Outlet,
    NavLink,
    useLocation,
    type NavLinkRenderProps,
} from "react-router";
import type { Location } from "react-router";
import {
    isNavGroup,
    paths,
    navigation,
    type NavEntry,
    flatNavigation,
} from "@/routes/navigation";
import type { ReactNode } from "react";
import ThemeToggle from "../common/theme-toggle";
import { Menu, MenuButton, MenuItem } from "@/components/navigation/menu";
import Icon from "@/components/common/icon";

function navClass({ isActive }: NavLinkRenderProps) {
    return `text-sm uppercase transition-colors ${
        isActive ? "font-bold text-ink" : "text-ink-muted hover:text-ink"
    }`;
}

function initialsOf(name: string): string {
    return name
        .split(" ")
        .filter(Boolean)
        .slice(0, 2)
        .map((part) => part.charAt(0).toUpperCase())
        .join("");
}

function mapNavigationEntry(
    entry: NavEntry,
    location: Location<any>,
): ReactNode {
    if (!isNavGroup(entry)) {
        return (
            <NavLink key={entry.to} to={entry.to} className={navClass}>
                {entry.label}
            </NavLink>
        );
    }

    const active = location.pathname.startsWith(entry.match);

    return (
        <Menu
            key={entry.label}
            triggerClassName={active ? "text-ink" : "text-ink-muted"}
            label={
                <span
                    className={`text-sm uppercase ${active ? "font-bold" : ""}`}
                >
                    {entry.label}
                </span>
            }
        >
            {entry.items.map((item) => (
                <MenuItem key={item.to} to={item.to}>
                    {item.label}
                </MenuItem>
            ))}
        </Menu>
    );
}

function RootLayout() {
    var auth = useAuth();
    var location = useLocation();

    const name =
        auth.user?.profile.name ?? auth.user?.profile.preferred_username ?? "";
    const email = auth.user?.profile.email ?? "";

    return (
        <div className="flex min-h-screen flex-col bg-canvas md:p-10">
            <div className="flex flex-1 flex-col border-border-strong bg-surface md:rounded-lg md:border">
                <header className="flex h-18 shrink-0 items-center gap-10 border-b border-border-strong px-6 md:px-7">
                    <NavLink
                        to={paths.dashboard}
                        className="flex items-center gap-3"
                    >
                        <span className="flex size-7 items-center justify-center rounded-[3px] bg-action text-action-ink">
                            <Icon name="logo" className="size-7" />
                        </span>
                        <span className="font-display text-[22px] leading-none font-bold text-ink">
                            FHS
                        </span>
                    </NavLink>
                    <nav className="hidden items-center gap-8 md:flex">
                        {navigation.map((entry) =>
                            mapNavigationEntry(entry, location),
                        )}
                    </nav>

                    <div className="ml-auto flex items-center gap-4">
                        <ThemeToggle />

                        <Menu
                            align="end"
                            chevron={false}
                            triggerClassName="text-ink md:hidden"
                            label={
                                <>
                                    <Icon name="menu" className="size-6" />
                                    <span className="sr-only">Menu</span>
                                </>
                            }
                        >
                            {flatNavigation.map((item) => (
                                <MenuItem key={item.to} to={item.to}>
                                    {item.label}
                                </MenuItem>
                            ))}
                        </Menu>

                        <Menu
                            align="end"
                            label={
                                <>
                                    <span className="flex size-7.5 items-center justify-center rounded-full bg-border-subtle text-[10px] font-bold text-ink">
                                        {initialsOf(name)}
                                    </span>
                                    <span className="hidden text-sm text-ink md:inline">
                                        {name}
                                    </span>
                                </>
                            }
                        >
                            <p className="px-3 py-2 text-xs text-ink-muted">
                                Signed in as {email || name}
                            </p>
                            <hr className="my-1 border-border-subtle" />
                            <MenuButton
                                onClick={() => void auth.signoutRedirect()}
                            >
                                Sign out
                            </MenuButton>
                        </Menu>
                    </div>
                </header>

                <main className="flex-1 px-6 py-8 md:px-10 md:py-9">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}

export default RootLayout;
