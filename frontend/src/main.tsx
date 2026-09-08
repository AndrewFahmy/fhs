import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { AuthProvider } from "react-oidc-context";
import { onSigninCallback, userManager } from "./api/user-manager.ts";
import App from "./App.tsx";

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
