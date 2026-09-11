import { UserManager, WebStorageStateStore } from "oidc-client-ts";

const authority = import.meta.env.VITE_OIDC_AUTHORITY;

if (!authority) {
    throw new Error(
        "VITE_OIDC_AUTHORITY is not set. The Aspire AppHost supplies it — run the solution with `aspire run`, not `bun run dev` alone.",
    );
}

export const userManager = new UserManager({
    authority,
    client_id: "fhs-spa",
    redirect_uri: `${window.location.origin}/`,
    post_logout_redirect_uri: `${window.location.origin}/`,
    response_type: "code",
    scope: "openid profile email",
    automaticSilentRenew: true,
    userStore: new WebStorageStateStore({ store: window.sessionStorage }),
});

export interface SigninState {
    returnTo: string;
}

/** Where to come back to after Keycloak: the page that needed the sign-in. */
export function signinState(): SigninState {
    return {
        returnTo: `${window.location.pathname}${window.location.search}`,
    };
}

export function returnToFrom(state: unknown): string {
    const returnTo =
        typeof state === "object" &&
        state !== null &&
        "returnTo" in state &&
        typeof state.returnTo === "string"
            ? state.returnTo
            : "";

    // In-app paths only: "//host" would leave the site.
    return returnTo.startsWith("/") && !returnTo.startsWith("//")
        ? returnTo
        : "/";
}
