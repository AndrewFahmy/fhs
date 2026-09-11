import { useAuth, useAutoSignin } from "react-oidc-context";
import { RouterProvider } from "react-router";
import { Notice } from "@/components/common/notice";
import { Button } from "@/components/controls/button";
import { router } from "@/routes";
import { signinState } from "@/utils/user-manager";

function App() {
    const auth = useAuth();
    const { isAuthenticated, error } = useAutoSignin({
        signinArgs: { state: signinState() },
    });

    if (error) {
        return (
            <Notice
                title="Sign-in failed"
                action={
                    <Button onClick={() => void auth.signinRedirect()}>
                        Try again
                    </Button>
                }
            >
                {error.message}
            </Notice>
        );
    }

    if (!isAuthenticated) {
        return <Notice title="Signing in...">Redirecting to Keycloak.</Notice>;
    }

    return <RouterProvider router={router} />;
}

export default App;
