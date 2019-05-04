clc;
tu0 = 0;
tu1 = -30;
tv0 = 0;
tv1 = 22;
u = 5;
v = 5;



for j = 0: v
    resV = tv0 - j*(tv0-tv1)/v
    for i = 0: u
        resU = tu0 - i*(tu0-tu1)/u 
    end
end