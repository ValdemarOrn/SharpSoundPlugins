% This function accepts coefficients of a laplace TF up to 4th order
% and returns coeffs. for a Z-plane TF
% See corresponding Mathematica file for more info
% b4s^4 + b3s^3 + b2s^2 + b1s + b0
% -------------------------------- = H(s)
% a4s^4 + a3s^3 + a2s^2 + a1s + a0

% returns

% b(1)z^4 + b(2)z^3 + b(3)z^2 + b(4)z + b(5)
% ------------------------------------------ = H(z)
% a(1)z^4 + a(2)z^3 + a(3)z^2 + a(4)z + a(5)

function sToz4(b4,b3,b2,b1,b0,a4,a3,a2,a1,a0,fs)


b(5) = b0 - 2*b1*fs + 4*b2*fs^2 - 8*b3*fs^3 + 16*b4*fs^4; % z^0
b(4) = 4*b0 - 4*b1*fs + 16*b3*fs^3 - 64*b4*fs^4; %z^1
b(3) = 6*b0 - 8*b2*fs^2 + 96*b4*fs^4; %z^2
b(2) = 4*b0 + 4*b1*fs - 16*b3*fs^3 - 64*b4*fs^4; %z^3
b(1) = b0 + 2*b1*fs + 4*b2*fs^2 + 8*b3*fs^3 + 16*b4*fs^4; %z^4

a(5) = a0 - 2*a1*fs + 4*a2*fs^2 - 8*a3*fs^3 + 16*a4*fs^4; %z^0
a(4) = 4*a0 - 4*a1*fs + 16*a3*fs^3 - 64*a4*fs^4; %z^1
a(3) = 6*a0 - 8*a2*fs^2 + 96*a4*fs^4; %z^2
a(2) = 4*a0 + 4*a1*fs - 16*a3*fs^3 - 64*a4*fs^4; %z^3
a(1) = a0 + 2*a1*fs + 4*a2*fs^2 + 8*a3*fs^3 + 16*a4*fs^4; %z^4

z = tf('z',1/fs);

sys2 = (b(1)*z^4 + b(2)*z^3 + b(3)*z^2 + b(4)*z + b(5)) / ( a(1)*z^4 + a(2)*z^3 + a(3)*z^2 + a(4)*z + a(5) )

figure(2)
bode(sys2);

end