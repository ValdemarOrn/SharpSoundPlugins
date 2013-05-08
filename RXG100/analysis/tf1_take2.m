clc

R1 = 68000
R2 = 1000000
C1 = 0.01e-6
C2 = 250e-12
s = tf('s');

a = R1 + 1/(s*C1);
b = 1/(s*C2+1/R2);
h = b/(a+b)

% Skv. Mathematica
h2 = (C1*R2*s)/(1 + (C1*R1+C1*R2+C2*R2)*s + C1*C2*R1*R2*s^2)

sToz2(0,C1*R2,0,1,(C1*R1+C1*R2+C2*R2),C1*C2*R1*R2,96000);

figure(1)
bode(h)

figure(2)
bode(h2)