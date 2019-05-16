y = [10,20,30,15];
a = bar(y);
labels = {'A', 'B', 'C', 'D'};
xt = get(gca, 'XTick');
text(xt, y, labels, 'HorizontalAlignment','center', 'VerticalAlignment','bottom')