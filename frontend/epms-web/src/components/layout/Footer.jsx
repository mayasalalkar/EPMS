function Footer({ className = '' }) {
  return (
    <footer className={`app-footer text-center text-secondary small py-3 ${className}`}>
      <span>&copy; EPMS</span>
      <span className="mx-2">&middot;</span>
      <span>Developed by Maya Salalkar</span>
    </footer>
  );
}

export default Footer;
