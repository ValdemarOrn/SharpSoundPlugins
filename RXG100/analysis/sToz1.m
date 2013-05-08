% This function accepts coefficients of a laplace TF up to 4th order
% and returns coeffs. for a Z-plane TF
% See corresponding Mathematica file for more info
% b1s + b0 
% --------- = H(s)
% a1s + a0 

% returns

% b(1)z + b(2)
% ------------- = H(z)
% a(1)z + a(2)

function sToz1(b1,b0,a1,a0,fs)


b(2) = b0 - 2*b1*fs; % z^0
b(1) = b0 + 2*b1*fs; %z^1

a(2) = a0 - 2*a1*fs; %z^0
a(1) = a0 + 2*a1*fs; %z^1

z = tf('z',1/fs);

sys2 = (b(1)*z + b(2) ) / ( a(1)*z + a(2) )

figure(2)
bode(sys2);

end