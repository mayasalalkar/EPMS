import { getAvatarColor, getInitials } from '../utils/helpers';

function Avatar({ name, seed, size = '' }) {
  return (
    <span
      className={`avatar ${size ? `avatar-${size}` : ''}`}
      style={{ backgroundColor: getAvatarColor(seed ?? name) }}
      title={name}
    >
      {getInitials(name)}
    </span>
  );
}

export default Avatar;
