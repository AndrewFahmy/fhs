import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import "@/index.css";
import { AuthProvider } from "react-oidc-context";
import { onSigninCallback, userManager } from "@/api/user-manager.ts";
import App from "@/App.tsx";

const queryClient = new QueryClient();

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <AuthProvider
            userManager={userManager}
            onSigninCallback={onSigninCallback}
        >
            <QueryClientProvider client={queryClient}>
                <App />
            </QueryClientProvider>
        </AuthProvider>
    </StrictMode>,
);
