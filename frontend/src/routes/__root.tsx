import type { QueryClient } from "@tanstack/react-query";
import {
    Link,
    Outlet,
    createRootRouteWithContext,
} from "@tanstack/react-router";

export const Route = createRootRouteWithContext<{
    queryClient: QueryClient;
}>()({ component: RootLayout });

function RootLayout() {
    return (
        <div className="bg-background text-foreground min-h-screen">
            <header className="border-b">
                <nav className="mx-auto flex max-w-screen-2xl gap-4 p-4">
                    <Link to="/" className="font-semibold">
                        Fault Handling System
                    </Link>
                </nav>
            </header>
            <main className="mx-auto max-w-screen-2xl p-4">
                <Outlet />
            </main>
        </div>
    );
}
