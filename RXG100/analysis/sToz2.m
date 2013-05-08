% This function accepts coefficients of a laplace TF up to 4th order
% and returns coeffs. for a Z-plane TF
% See corresponding Mathematica file for more info
% b2s^2 + b1s + b0 
% ----------------- = H(s)
% a2s^2 + a1s + a0 

% returns

% b(1)z^2 + b(2)z + b(3)
% ----------------------- = H(z)
% a(1)z^2 + a(2)z + a(3)

function sToz2(b2,b1,b0,a2,a1,a0,fs)


b(3) = b0 - 2*b1*fs + 4*b2*fs^2; % z^0
b(2) = 2*b0 - 8*b2*fs^2; %z^1
b(1) = b0 + 2*b1*fs + 4*b2*fs^2; %z^2

a(3) = a0 - 2*a1*fs + 4*a2*fs^2; %z^0
a(2) = 2*a0 - 8*a2*fs^2; %z^1
a(1) = a0 + 2*a1*fs + 4*a2*fs^2; %z^2

z = tf('z',1/fs);

sys2 = (b(1)*z^2 + b(2)*z + b(3) ) / ( a(1)*z^2 + a(2)*z + a(3) )

figure(2)
bode(sys2);

end