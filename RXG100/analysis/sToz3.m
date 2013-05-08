% This function accepts coefficients of a laplace TF up to 4th order
% and returns coeffs. for a Z-plane TF
% See corresponding Mathematica file for more info
% b3s^3 + b2s^2 + b1s + b0
% -------------------------------- = H(s)
% a3s^3 + a2s^2 + a1s + a0

% returns

% b(1)z^3 + b(2)z^2 + b(3)z + b(4)
% -------------------------------- = H(z)
% a(1)z^3 + a(2)z^2 + a(3)z + a(4)

function sToz3(b3,b2,b1,b0,a3,a2,a1,a0,fs)


b(4) = b0 - 2*b1*fs + 4*b2*fs^2 - 8*b3*fs^3; % z^0
b(3) = 3*b0 - 2*b1*fs - 4*b2*fs^2 + 24*b3*fs^3; %z^1
b(2) = 3*b0 + 2*b1*fs - 4*b2*fs^2 - 24*b3*fs^3; %z^2
b(1) = b0 + 2*b1*fs + 4*b2*fs^2 + 8*b3*fs^3; %z^3

a(4) = a0 - 2*a1*fs + 4*a2*fs^2 - 8*a3*fs^3; %z^0
a(3) = 3*a0 - 2*a1*fs - 4*a2*fs^2 + 24*a3*fs^3; %z^1
a(2) = 3*a0 + 2*a1*fs - 4*a2*fs^2 - 24*a3*fs^3; %z^2
a(1) = a0 + 2*a1*fs + 4*a2*fs^2 + 8*a3*fs^3; %z^3

z = tf('z',1/fs);

sys2 = (b(1)*z^3 + b(2)*z^2 + b(3)*z + b(4) ) / ( a(1)*z^3 + a(2)*z^2 + a(3)*z + a(4) )

figure(2)
bode(sys2);

end