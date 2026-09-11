import { useAuth } from "react-oidc-context";

/**
 * Whether the signed-in user holds the realm's `admin` role, read from the ID token.
 * It only decides what the UI shows; the API enforces the role on every request.
 */
export function useIsAdmin(): boolean {
    const auth = useAuth();
    const roles = auth.user?.profile.roles;

    return Array.isArray(roles) && roles.includes("admin");
}
