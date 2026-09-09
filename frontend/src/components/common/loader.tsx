function Loader({ className = "" }: { className?: string }) {
    return (
        <div
            className={`animate-pulse rounded-sm bg-surface-subtle ${className}`}
        />
    );
}

export default Loader;
