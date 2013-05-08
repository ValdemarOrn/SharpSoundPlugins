R1 = 560
R2 = 47
C1 = 4.7e-6
C2 = 2.2e-6
Gain = 150e3 * 0.99
C3 = 100e-12


s = tf('s');

b = 1 + (C1*Gain + C2*Gain + C3*Gain + C1*R1 + C2*R2)*s + (C1*C2*Gain*R1 + C1*C3*Gain*R1 + C1*C2*Gain*R2 + C2*C3*Gain*R2 + C1*C2*R1*R2)*s^2 + C1*C2*C3*Gain*R1*R2*s^3;
a = 1 + (C3*Gain + C1*R1 + C2*R2)*s + (C1* C3*Gain*R1 + C2*C3*Gain*R2 + C1*C2*R1*R2)*s^2 + C1*C2*C3*Gain*R1*R2*s^3;

t1 = b/a;

t2 = s/(s + 7.5*2*pi)
t3 = s/(s + 1.5*2*pi)

t4 = t1*t2*t3;

bode(t4)