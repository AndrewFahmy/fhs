import createClient from "openapi-fetch";
import type { paths } from "./schema";

const baseUrl = import.meta.env.VITE_API_URL;

if (!baseUrl) {
    throw new Error(
        "VITE_API_URL is not set. The Aspire AppHost supplies it — run through `aspire run`, not `bun run dev` alone.",
    );
}

export const api = createClient<paths>({ baseUrl });
