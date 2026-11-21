function Loader({ text = 'Loading...' }) {
  return (
    <div className="d-flex flex-column align-items-center justify-content-center py-5 text-secondary">
      <div className="spinner-border text-primary mb-2" role="status" aria-hidden="true"></div>
      <span className="small">{text}</span>
    </div>
  );
}

export default Loader;
