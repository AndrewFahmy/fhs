import { useCallback, useEffect, useState } from "react";
import axios, { type AxiosResponse } from "axios";
import { userManager } from "@/api/user-manager";
import { type RetriedRequestConfig, ApiError } from "@/api/types";
import { toApiError } from "@/api/helpers/common";
import type { User } from "oidc-client-ts";

const baseUrl = import.meta.env.VITE_API_URL;

if (!baseUrl) {
    throw new Error(
        "VITE_API_URL is not set. The Aspire AppHost supplies it — run through `aspire run`, not `npm run dev` alone.",
    );
}

const apiClient = axios.create({
    baseURL: baseUrl,
    headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
    },
});

/** One renewal at a time, however many requests hit 401 together. */
let renewal: Promise<User | null> | null = null;

function renewSession(): Promise<User | null> {
    renewal ??= userManager
        .signinSilent()
        .catch(() => null)
        .finally(() => {
            renewal = null;
        });

    return renewal;
}

// request interceptor
apiClient.interceptors.request.use(async (config) => {
    const user = await userManager.getUser();

    const token = user?.expired ? null : user?.access_token;

    if (token) {
        config.headers.set("Authorization", `Bearer ${token}`);
    } else {
        config.headers.delete("Authorization");
    }
    return config;
});

// response interceptor
apiClient.interceptors.response.use(
    (response) => response,
    async (cause: unknown) => {
        if (!axios.isAxiosError(cause) || cause.response?.status !== 401) {
            return Promise.reject(toApiError(cause));
        }

        const config = cause.config as RetriedRequestConfig | undefined;

        if (!config || config.retried) {
            return Promise.reject(toApiError(cause));
        }

        config.retried = true;
        const renewed = await renewSession();

        if (renewed) {
            return apiClient.request(config);
        }

        await userManager.signinRedirect();

        return Promise.reject(toApiError(cause));
    },
);

// hooks for HTTP methods
export function useGet<T>(url: string) {
    const [data, setData] = useState<T | null>(null);
    const [error, setError] = useState<ApiError | null>(null);
    const [loading, setLoading] = useState(true);

    const fetchData = useCallback(async () => {
        setLoading(true);
        setError(null);

        try {
            const response = await apiClient.get<T>(url);
            setData(response.data);
        } catch (cause) {
            setError(toApiError(cause));
        } finally {
            setLoading(false);
        }
    }, [url]);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    return { data, error, loading, refetch: fetchData };
}

export function useDelete<TResponse = void>(url: string) {
    const [error, setError] = useState<ApiError | null>(null);
    const [loading, setLoading] = useState(false);

    async function del(): Promise<AxiosResponse<TResponse> | null> {
        setLoading(true);
        setError(null);

        try {
            return await apiClient.delete<TResponse>(url);
        } catch (cause) {
            setError(toApiError(cause));
            return null;
        } finally {
            setLoading(false);
        }
    }

    return { del, error, loading };
}

export function usePost<TResponse = void, TBody = unknown>(url: string) {
    const [error, setError] = useState<ApiError | null>(null);
    const [loading, setLoading] = useState(false);

    async function post(
        body?: TBody,
    ): Promise<AxiosResponse<TResponse> | null> {
        setLoading(true);
        setError(null);

        try {
            return await apiClient.post<TResponse>(url, body);
        } catch (cause) {
            setError(toApiError(cause));
            return null;
        } finally {
            setLoading(false);
        }
    }

    return { post, error, loading };
}

export function usePut<TResponse = void, TBody = unknown>(url: string) {
    const [error, setError] = useState<ApiError | null>(null);
    const [loading, setLoading] = useState(false);

    async function put(body?: TBody): Promise<AxiosResponse<TResponse> | null> {
        setLoading(true);
        setError(null);

        try {
            return await apiClient.put<TResponse>(url, body);
        } catch (cause) {
            setError(toApiError(cause));
            return null;
        } finally {
            setLoading(false);
        }
    }

    return { put, error, loading };
}

export function usePatch<TResponse = void, TBody = unknown>(url: string) {
    const [error, setError] = useState<ApiError | null>(null);
    const [loading, setLoading] = useState(false);

    async function patch(
        body?: TBody,
    ): Promise<AxiosResponse<TResponse> | null> {
        setLoading(true);
        setError(null);

        try {
            return await apiClient.patch<TResponse>(url, body);
        } catch (cause) {
            setError(toApiError(cause));
            return null;
        } finally {
            setLoading(false);
        }
    }

    return { patch, error, loading };
}
