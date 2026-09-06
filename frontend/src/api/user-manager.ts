import { UserManager, WebStorageStateStore } from "oidc-client-ts";

const authority = import.meta.env.VITE_OIDC_AUTHORITY;

if (!authority) {
    throw new Error(
        "VITE_OIDC_AUTHORITY is not set. The Aspire AppHost supplies it — run through `aspire run`, not `bun run dev` alone.",
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

export function onSigninCallback(): void {
    window.history.replaceState({}, document.title, window.location.pathname);
}

export function onSignoutCallback(): void {
    window.sessionStorage.clear();
    window.history.replaceState({}, document.title, window.location.pathname);
}
