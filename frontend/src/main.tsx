import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import type { User } from "oidc-client-ts";
import "@/index.css";
import { AuthProvider } from "react-oidc-context";
import { returnToFrom, userManager } from "@/utils/user-manager";
import App from "@/App.tsx";
import { router } from "@/routes";

/** Back to the page that sent us to Keycloak; replacing the entry also drops ?code&state. */
function onSigninCallback(user: User | undefined) {
    void router.navigate(returnToFrom(user?.state), { replace: true });
}

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <AuthProvider
            userManager={userManager}
            onSigninCallback={onSigninCallback}
        >            
                <App />
        </AuthProvider>
    </StrictMode>,
);
